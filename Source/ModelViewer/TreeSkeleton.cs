﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Boogie.ModelViewer
{
  internal class SkeletonItem
  {
    readonly string name;
    readonly List<SkeletonItem> children = new List<SkeletonItem>();
    internal readonly IDisplayNode[] displayNodes;
    internal bool[] isPrimary;
    internal readonly SkeletonItem parent;
    internal readonly Main main;
    internal readonly int level;
    internal bool expanded, wasExpanded;
    internal bool isMatch;

    public void Iter(Action<SkeletonItem> handler)
    {
      handler(this);
      children.ForEach(u => u.Iter(handler));
    }

    public IEnumerable<SkeletonItem> RecChildren
    {
      get
      {
        if (expanded) {
          foreach (var c in children) {
            yield return c;
            foreach (var ch in c.RecChildren)
              yield return ch;
          }
        }
      }
    }

    public SkeletonItem[] PopulateRoot(IEnumerable<IState> states)
    {
      var i = 0;
      foreach (var s in states) {
        displayNodes[i++] = new ContainerNode<IDisplayNode>(this.name, x => x, s.Nodes);
      }

      return BfsExpand(this);
    }

    public SkeletonItem(Main m, int stateCount)
    {
      name = "<root>";
      main = m;
      displayNodes = new IDisplayNode[stateCount];
      isPrimary = new bool[stateCount];
    }

    internal SkeletonItem(string n, SkeletonItem par)
      : this(par.main, par.displayNodes.Length)
    {
      parent = par;
      name = n;
      level = par.level + 1;
    }

    public bool Expandable
    {
      get { return displayNodes.Any(d => d != null && d.Children.Count() > 0); }
    }

    public bool Expanded
    {
      get { return expanded; }
      set
      {
        expanded = value;
        if (expanded)
          ComputeChildren();
      }
    }

    static SkeletonItem[] BfsExpand(SkeletonItem skel)
    {
      for (int i = 0; i < skel.displayNodes.Length; ++i)
        BfsExpandCore(skel, i);

      var workItems = new Stack<SkeletonItem>();
      var allNodes = new List<SkeletonItem>();
      workItems.Push(skel);

      while (workItems.Count > 0) {
        var s = workItems.Pop();
        if (!s.isPrimary.Any())
          continue;
        allNodes.Add(s);
        s.children.Iter(workItems.Push);
      }

      return allNodes.ToArray();
    }

    static void BfsExpandCore(SkeletonItem skel, int idx)
    {
      var visited = new HashSet<Model.Element>();
      var workItems = new Queue<SkeletonItem>();

      workItems.Enqueue(skel);
      while (workItems.Count > 0) {
        var s = workItems.Dequeue();
        if (s.displayNodes[idx] == null)
          continue;
        var e = s.displayNodes[idx].Element;
        s.isPrimary[idx] = true;
        if (e != null) {
          if (visited.Contains(e))
            continue;
          visited.Add(e);
        }
        s.ComputeChildren();
        s.children.Iter(workItems.Enqueue);
      }
    }

    private void ComputeChildren()
    {
      if (wasExpanded) return;
      wasExpanded = true;

      var created = new Dictionary<string, SkeletonItem>();
      var names = new List<string>();
      for (int i = 0; i < displayNodes.Length; ++i) {
        var dn = displayNodes[i];
        if (dn == null) continue;
        foreach (var child in dn.Children) {
          SkeletonItem skelChild;
          var name = child.Name;
          if (!created.TryGetValue(name, out skelChild)) {
            skelChild = new SkeletonItem(child.Name, this);
            created.Add(name, skelChild);
            names.Add(name);

          }
          skelChild.displayNodes[i] = child;
        }
      }

      foreach (var name in main.langModel.SortFields(names)) {
        children.Add(created[name]);
      }
    }

    public bool MatchesWords(string[] words, Model.Element[] elts, Model.Element eq, int stateId)
    {
      var node = displayNodes[stateId];
      if (node == null)
        return false;
      var s1 = node.Name.ToLower();
      var s2 = node.Value.ToLower();

      if (eq != null && node.Element != eq)
        return false;

      foreach (var w in words) {
        if (!s1.Contains(w) && !s2.Contains(w))
          return false;
      }

      foreach (var e in elts) {
        if (!node.References.Contains(e))
          return false;
      }

      return true;
    }
  }

}