using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace GadgetCore.Util
{
    /// <summary>
    /// Represents a tree-like data structure where the nodes can have multiple parents.
    /// </summary>
    public class MultiTreeList<T> : IList<MultiTreeList<T>>
    {
        /// <summary>
        /// The value of this tree node.
        /// </summary>
        public readonly T Value;
        /// <summary>
        /// The parents of this tree node.
        /// </summary>
        public ReadOnlyCollection<MultiTreeList<T>> Parents;
        /// <summary>
        /// The parents of this tree node. Mutable, unless <see cref="IsReadOnly"/>
        /// </summary>
        protected IList<MultiTreeList<T>> m_Parents;
        /// <summary>
        /// The child nodes of this node.
        /// </summary>
        protected IList<MultiTreeList<T>> Nodes;

        /// <summary>
        /// Gets the number of child nodes to this node.
        /// </summary>
        public int Count => Nodes.Count;

        /// <summary>
        /// Gets a value indicating whether this <see cref="MultiTreeList{T}"/> is read-only.
        /// </summary>
        public bool IsReadOnly => Nodes.IsReadOnly;

        /// <summary>
        /// Gets or sets the node at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the node to get or set.</param>
        /// <returns>The node at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">index is less than 0, or index is equal to or greater than <see cref="Count"/>.</exception>
        public MultiTreeList<T> this[int index] { get => Nodes[index]; set { Nodes[index] = value; if (!value.m_Parents.Contains(this)) value.m_Parents.Add(this); } }

        /// <summary>
        /// Constructs a new node with the given value.
        /// </summary>
        public MultiTreeList(T Value, params MultiTreeList<T>[] ChildNodes) : base()
        {
            this.Value = Value;
            m_Parents = new List<MultiTreeList<T>>();
            Parents = new ReadOnlyCollection<MultiTreeList<T>>(m_Parents);
            Nodes = new List<MultiTreeList<T>>(ChildNodes);
            if (Parents.Any(x => x.IsReadOnly) || Nodes.Any(x => x.IsReadOnly)) MakeReadOnly();
            foreach (MultiTreeList<T> node in Nodes) if (!node.m_Parents.Contains(this)) node.m_Parents.Add(this);
        }

        /// <summary>
        /// Constructs a <see cref="MultiTreeList{T}"/> from a dictionary representative of the relationships between values. The keys are parents, and the values are children.
        /// </summary>
        public MultiTreeList(IDictionary<T, IEnumerable<T>> data, T parentValue = default) : base()
        {
            Value = parentValue;
            IDictionary<T, MultiTreeList<T>> nodifiedData = data.ToDictionary(x => x.Key, x => new MultiTreeList<T>(x.Key));
            foreach (T t in nodifiedData.Keys)
            {
                foreach (T child in data[t])
                {
                    nodifiedData[t].Add(nodifiedData[child]);
                }
            }
            m_Parents = new List<MultiTreeList<T>>();
            Parents = new ReadOnlyCollection<MultiTreeList<T>>(m_Parents);
            Nodes = nodifiedData.Values.Where(x => !nodifiedData.Values.SelectMany(y => y).Contains(x)).ToList();
        }

        /// <summary>
        /// Checks if the given value is contained anywhere in the tree.
        /// </summary>
        public bool Contains(T item)
        {
            return (Value == null ? item == null : EqualityComparer<T>.Default.Equals(item, Value)) || Nodes.Any(x => x.Contains(item));
        }

        /// <summary>
        /// Finds the first node containing the specified value. Returns null if the value is nowhere in the tree.
        /// </summary>
        public MultiTreeList<T> Find(T item)
        {
            return (Value == null ? item == null : EqualityComparer<T>.Default.Equals(item, Value)) ? this : Nodes.FirstOrDefault(x => x.Contains(item))?.Find(item);
        }

        /// <summary>
        /// Finds all nodes containing the specified value. Returns an empty list if the value is nowhere in the tree.
        /// </summary>
        public List<MultiTreeList<T>> FindAll(T item)
        {
            List<MultiTreeList<T>> nodes = new List<MultiTreeList<T>>();
            if (Value == null ? item == null : EqualityComparer<T>.Default.Equals(item, Value)) nodes.Add(this);
            nodes.AddRange(Nodes.SelectMany(x => x.FindAll(item)));
            return nodes.Distinct().ToList();
        }

        /// <summary>
        /// Lists all nodes that are this node's children, including this node itself.
        /// </summary>
        public List<MultiTreeList<T>> ListAllNodes()
        {
            List<MultiTreeList<T>> nodes = new List<MultiTreeList<T>>
            {
                this
            };
            nodes.AddRange(Nodes.SelectMany(x => x.ListAllNodes()));
            return nodes;
        }

        /// <summary>
        /// Sorts the children of this node.
        /// </summary>
        public void Sort(Comparison<MultiTreeList<T>> comparison)
        {
            ArrayList.Adapter((IList)Nodes).Sort(new ComparisonComparer<MultiTreeList<T>>(comparison));
        }

        /// <summary>
        /// Gets all top-level parents to this node.
        /// </summary>
        public List<MultiTreeList<T>> GetTopLevelParents()
        {
            if (Parents.Count == 0) return new List<MultiTreeList<T>>(new MultiTreeList<T>[] { this });
            return Parents.SelectMany(x => x.GetTopLevelParents()).ToList();
        }

        /// <summary>
        /// Gets the one and only top-level parent of this tree. If there are multiple, creates a new top-level parent and adds the previous top-level parents as children to the new one.
        /// </summary>
        /// <param name="topLevelValue">The value for the new top-level parent to have, if one is created.</param>
        public MultiTreeList<T> GetTopLevelParent(T topLevelValue = default)
        {
            List<MultiTreeList<T>> parents = GetTopLevelParents();
            if (parents.Count > 1)
            {
                return new MultiTreeList<T>(topLevelValue, parents.ToArray());
            }
            return parents[0];
        }

        /// <summary>
        /// Makes this <see cref="MultiTreeList{T}"/> read-only. This is a permanent operation, it cannot be undone.
        /// </summary>
        public MultiTreeList<T> MakeReadOnly()
        {
            if (!Nodes.IsReadOnly)
            {
                Nodes = new ReadOnlyCollection<MultiTreeList<T>>(Nodes);
                m_Parents = Parents;
            }
            foreach (MultiTreeList<T> child in Nodes) if (!child.IsReadOnly) child.MakeReadOnly();
            foreach (MultiTreeList<T> parent in Parents) if (!parent.IsReadOnly) parent.MakeReadOnly();
            return this;
        }

        /// <summary>
        /// Flattens the tree into the form of a list, depth-first.
        /// </summary>
        public List<T> FlattenByDepth()
        {
            List<T> flatList = new List<T>();
            FlattenInternalByDepth(flatList);
            return flatList;
        }

        /// <summary>
        /// A recursive function used for flattening the tree, depth-first.
        /// </summary>
        protected void FlattenInternalByDepth(List<T> flatList)
        {
            flatList.Add(Value);
            foreach (MultiTreeList<T> node in this)
            {
                node.FlattenInternalByDepth(flatList);
            }
        }

        /// <summary>
        /// Flattens the tree into the form of a list of unique values, depth-first.
        /// </summary>
        public List<T> FlattenUniqueByDepth()
        {
            List<T> flatList = new List<T>();
            FlattenUniqueInternalByDepth(flatList);
            return flatList;
        }

        /// <summary>
        /// A recursive function used for flattening the unique values of the tree, depth-first.
        /// </summary>
        protected void FlattenUniqueInternalByDepth(List<T> flatList)
        {
            if (!flatList.Contains(Value)) flatList.Add(Value);
            foreach (MultiTreeList<T> node in this)
            {
                node.FlattenUniqueInternalByDepth(flatList);
            }
        }

        /// <summary>
        /// Flattens the tree into the form of a list, breadth-first.
        /// </summary>
        public List<T> FlattenByBreadth()
        {
            List<T> flatList = new List<T>
            {
                Value
            };
            FlattenInternalByBreadth(flatList, Nodes);
            return flatList;
        }

        /// <summary>
        /// A recursive function used for flattening the tree, breadth-first.
        /// </summary>
        protected void FlattenInternalByBreadth(List<T> flatList, IList<MultiTreeList<T>> listToTraverse)
        {
            foreach (MultiTreeList<T> node in listToTraverse)
            {
                if (node.Parents.All(x => flatList.Contains(x.Value))) flatList.Add(node.Value);
            }
            if (listToTraverse.Count > 0) FlattenInternalByBreadth(flatList, listToTraverse.SelectMany(x => x).ToList());
        }

        /// <summary>
        /// Flattens the tree into the form of a list of unique values, breadth-first.
        /// </summary>
        public List<T> FlattenUniqueByBreadth()
        {
            List<T> flatList = new List<T>
            {
                Value
            };
            FlattenUniqueInternalByBreadth(flatList, Nodes);
            return flatList;
        }

        /// <summary>
        /// A recursive function used for flattening the unique values of the tree, breadth-first.
        /// </summary>
        protected void FlattenUniqueInternalByBreadth(List<T> flatList, IList<MultiTreeList<T>> listToTraverse)
        {
            foreach (MultiTreeList<T> node in listToTraverse)
            {
                if (!flatList.Contains(node.Value) && node.Parents.All(x => flatList.Contains(x.Value))) flatList.Add(node.Value);
            }
            if (listToTraverse.Count > 0) FlattenUniqueInternalByBreadth(flatList, listToTraverse.SelectMany(x => x).ToList());
        }

        /// <summary>
        /// Searches for the specified child node and returns the zero-based index of the first occurrence within the child nodes.
        /// </summary>
        public int IndexOf(MultiTreeList<T> item)
        {
            return Nodes.IndexOf(item);
        }

        /// <summary>
        /// Inserts a node into this <see cref="MultiTreeList{T}"/> at the specified index.
        /// </summary>
        public void Insert(int index, MultiTreeList<T> item)
        {
            Nodes.Insert(index, item);
            if (!item.m_Parents.Contains(this)) item.m_Parents.Add(this);
            if (!IsReadOnly && item.IsReadOnly) MakeReadOnly();
        }

        /// <summary>
        /// Removes this node from the tree.
        /// </summary>
        public void RemoveFromTree(bool inheritParents = true, bool removeOrphanedChildren = true)
        {
            foreach (MultiTreeList<T> childNode in Nodes.ToList())
            {
                Remove(childNode);
                if (inheritParents)
                {
                    childNode.m_Parents = childNode.Parents.Union(Parents).ToList();
                }
                if (removeOrphanedChildren)
                {
                    if (childNode.Parents.Count == 0) childNode.RemoveFromTree(true);
                }
            }
            foreach (MultiTreeList<T> parentNode in Parents.ToList())
            {
                parentNode.Remove(this);
            }
        }

        /// <summary>
        /// Removes the node at the specified index in the <see cref="MultiTreeList{T}"/>
        /// </summary>
        public void RemoveAt(int index)
        {
            this[index].m_Parents.Remove(this);
            Nodes.RemoveAt(index);
        }

        /// <summary>
        /// Adds a node to the end of the <see cref="MultiTreeList{T}"/>
        /// </summary>
        public void Add(MultiTreeList<T> item)
        {
            Nodes.Add(item);
            if (!item.m_Parents.Contains(this)) item.m_Parents.Add(this);
            if (!IsReadOnly && item.IsReadOnly) MakeReadOnly();
        }

        /// <summary>
        /// Removes all nodes from the <see cref="MultiTreeList{T}"/>.
        /// </summary>
        public void Clear()
        {
            foreach (MultiTreeList<T> node in Nodes) node.m_Parents.Remove(this);
            Nodes.Clear();
        }

        /// <summary>
        /// Determines whether a node is in the <see cref="MultiTreeList{T}"/>.
        /// </summary>
        public bool Contains(MultiTreeList<T> item)
        {
            return Nodes.Contains(item);
        }

        /// <summary>
        /// Copes the children of this <see cref="MultiTreeList{T}"/> to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        public void CopyTo(MultiTreeList<T>[] array, int arrayIndex)
        {
            Nodes.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurance of a specific node from the <see cref="MultiTreeList{T}"/>.
        /// </summary>
        public bool Remove(MultiTreeList<T> item)
        {
            item.m_Parents.Remove(this);
            return Nodes.Remove(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="MultiTreeList{T}"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<MultiTreeList<T>> GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }
    }
}
