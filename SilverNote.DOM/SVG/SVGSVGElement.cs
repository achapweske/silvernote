/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.CSS;
using DOM.Events;

namespace DOM.SVG
{
    public interface SVGSVGElement : SVGElement, SVGTests, SVGLangSpace, SVGExternalResourcesRequired, SVGStylable, SVGLocatable, SVGFitToViewBox,
        SVGZoomAndPan, DocumentEvent, ViewCSS, DocumentCSS
    {
        SVGAnimatedLength X { get; }
        SVGAnimatedLength Y { get; }
        SVGAnimatedLength Width { get; }
        SVGAnimatedLength Height { get; }
        string ContentScriptType { get; set; }
        string ContentStyleType { get; set; }
        SVGRect ViewPort { get; }
        double PixelUnitToMillimeterX { get; }
        double PixelUnitToMillimeterY { get; }
        double ScreenPixelToMillimeterX { get; }
        double ScreenPixelToMillimeterY { get; }
        bool UseCurrentView { get; }
        SVGViewSpec CurrentView { get; }
        double CurrentScale { get; set; }
        SVGPoint CurrentTranslate { get; }
        
        int SuspendRedraw(int maxWaitMilliseconds);
        void UnsuspendRedraw(int suspendHandleID);
        void UnsuspendRedrawAll();
        void ForceRedraw();
        void PauseAnimations();
        void UnpauseAnimations();
        bool AnimationsPaused();
        double GetCurrentTime();
        void SetCurrentTime(double seconds);
        NodeList GetIntersectionList(SVGRect rect, SVGElement referenceElement);
        NodeList GetEnclosureList(SVGRect rect, SVGElement referenceElement);
        bool CheckIntersection(SVGElement element, SVGRect rect);
        bool CheckEnclosure(SVGElement element, SVGRect rect);
        void DeselectAll();
        SVGNumber CreateSVGNumber();
        SVGLength CreateSVGLength();
        SVGAngle CreateSVGAngle();
        SVGPoint CreateSVGPoint();
        SVGMatrix CreateSVGMatrix();
        SVGRect CreateSVGRect();
        SVGTransform CreateSVGTransform();
        SVGTransform CreateSVGTransformFromMatrix(SVGMatrix matrix);
        Element GetElementById(string elementId);
    }
}
