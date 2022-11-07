﻿using Blazor.Diagrams.Core.Anchors;
using Blazor.Diagrams.Core.Geometry;
using System;
using System.Collections.Generic;
using SvgPathProperties;
using Blazor.Diagrams.Core.PathGenerators;
using Blazor.Diagrams.Core.Routers;

namespace Blazor.Diagrams.Core.Models.Base;

public abstract class BaseLinkModel : SelectableModel, IHasBounds, ILinkable
{
    private readonly List<BaseLinkModel> _links = new();
    
    public event Action<BaseLinkModel, Anchor, Anchor>? SourceChanged;
    public event Action<BaseLinkModel, Anchor, Anchor>? TargetChanged;

    protected BaseLinkModel(Anchor source, Anchor target)
    {
        Source = source;
        Target = target;
    }

    protected BaseLinkModel(string id, Anchor source, Anchor target) : base(id)
    {
        Source = source;
        Target = target;
    }

    public Anchor Source { get; private set; }
    public Anchor Target { get; private set; }
    public Diagram? Diagram { get; internal set; }
    public PathGeneratorResult GeneratedPathResult { get; private set; } = PathGeneratorResult.Empty;
    public SvgPath[] Paths => GeneratedPathResult.Paths;
    public bool IsAttached => Source is not PositionAnchor && Target is not PositionAnchor;
    public Router? Router { get; set; }
    public PathGenerator? PathGenerator { get; set; }
    public LinkMarker? SourceMarker { get; set; }
    public LinkMarker? TargetMarker { get; set; }
    public bool Segmentable { get; set; } = false;
    public List<LinkVertexModel> Vertices { get; } = new();
    public List<LinkLabelModel> Labels { get; } = new();
    public IReadOnlyList<BaseLinkModel> Links => _links;

    public override void Refresh()
    {
        GeneratePath();
        base.Refresh();
    }

    public void RefreshLinks()
    {
        foreach (var link in Links)
        {
            link.Refresh();
        }
    }

    public void SetSource(Anchor anchor)
    {
        ArgumentNullException.ThrowIfNull(anchor, nameof(anchor));

        if (Source == anchor)
            return;

        var old = Source;
        Source = anchor;
        SourceChanged?.Invoke(this, old, Source);
    }

    public void SetTarget(Anchor anchor)
    {
        if (Target == anchor)
            return;

        var old = Target;
        Target = anchor;
        TargetChanged?.Invoke(this, old, Target);
    }

    public Rectangle? GetBounds()
    {
        if (Paths.Length == 0)
            return null;

        var minX = double.PositiveInfinity;
        var minY = double.PositiveInfinity;
        var maxX = double.NegativeInfinity;
        var maxY = double.NegativeInfinity;

        foreach (var path in Paths)
        {
            var bbox = path.GetBBox();
            minX = Math.Min(minX, bbox.Left);
            minY = Math.Min(minY, bbox.Top);
            maxX = Math.Max(maxX, bbox.Right);
            maxY = Math.Max(maxY, bbox.Bottom);
        }

        return new Rectangle(minX, minY, maxX, maxY);
    }

    public bool CanAttachTo(ILinkable other) => true;

    private void GeneratePath()
    {
        if (Diagram != null)
        {
            var router = Router ?? Diagram.Options.Links.DefaultRouter;
            var pathGenerator = PathGenerator ?? Diagram.Options.Links.DefaultPathGenerator;
            var route = router.GetRoute(Diagram, this);
            var source = Source.GetPosition(this, route);
            var target = Target.GetPosition(this, route);
            if (source != null && target != null)
            {
                GeneratedPathResult = pathGenerator.GetResult(Diagram, this, route, source, target);
                return;
            }
        }

        GeneratedPathResult = PathGeneratorResult.Empty;
    }

    void ILinkable.AddLink(BaseLinkModel link) => _links.Add(link);

    void ILinkable.RemoveLink(BaseLinkModel link) => _links.Remove(link);
}
