﻿using System;
using System.Threading.Tasks;
using Blazor.Diagrams.Core.Geometry;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.Diagrams.Extensions;

public static class JSRuntimeExtensions
{
    public static async Task<Rectangle> GetBoundingClientRect(this IJSRuntime jsRuntime, ElementReference element)
    {
        try
        {
            return await jsRuntime.InvokeAsync<Rectangle>("ZBlazorDiagrams.getBoundingClientRect", element);
        }
        catch {
            // Ignore, likely disposed
        }
        return Rectangle.Zero;
    }

    public static async Task ObserveResizes<T>(this IJSRuntime jsRuntime, ElementReference element,
        DotNetObjectReference<T> reference) where T : class
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZBlazorDiagrams.observe", element, reference, element.Id);
        }
        catch (ObjectDisposedException)
        {
            // Ignore, DotNetObjectReference was likely disposed
        }
    }

    public static async Task UnobserveResizes(this IJSRuntime jsRuntime, ElementReference element)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZBlazorDiagrams.unobserve", element, element.Id);
        }
        catch
        {
            // Ignore, DotNetObjectReference was likely disposed
        }
    }
}