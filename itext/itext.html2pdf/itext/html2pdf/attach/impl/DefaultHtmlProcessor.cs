/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS

    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/

    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.

    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.

    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.

    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com */
using System;
using System.Collections.Generic;
using iText.Html2pdf;
using iText.Html2pdf.Attach;
using iText.Html2pdf.Css;
using iText.Html2pdf.Css.Apply;
using iText.Html2pdf.Css.Resolve;
using iText.Html2pdf.Html;
using iText.Html2pdf.Html.Node;
using iText.IO.Log;
using iText.IO.Util;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Versions.Attributes;
using iText.Kernel;

namespace iText.Html2pdf.Attach.Impl {
    public class DefaultHtmlProcessor : IHtmlProcessor {
        private static readonly ILogger logger = LoggerFactory.GetLogger(typeof(iText.Html2pdf.Attach.Impl.DefaultHtmlProcessor
            ));

        private static readonly ICollection<String> ignoredTags = JavaCollectionsUtil.UnmodifiableSet(new HashSet<
            String>(iText.IO.Util.JavaUtil.ArraysAsList(TagConstants.HEAD, TagConstants.STYLE, TagConstants.TBODY)
            ));

        private static readonly ICollection<String> ignoredCssTags = JavaCollectionsUtil.UnmodifiableSet(new HashSet
            <String>(iText.IO.Util.JavaUtil.ArraysAsList(TagConstants.BR, TagConstants.TITLE, TagConstants.META, TagConstants
            .TR)));

        private ProcessorContext context;

        private ICssResolver cssResolver;

        private INode root;

        private ResourceResolver resourceResolver;

        private IList<IPropertyContainer> roots;

        private ITagWorkerFactory tagWorkerFactory;

        public DefaultHtmlProcessor(INode node, ICssResolver cssResolver, ResourceResolver resourceResolver) {
            // The tags that do not map into any workers and are deliberately excluded from the logging
            // TODO <tbody> is not supported. Styles will be propagated anyway
            // The tags we do not want to apply css to and therefore exclude from the logging
            // Content from <tr> is thrown upwards to parent, in other cases css is inherited anyway
            this.cssResolver = cssResolver;
            this.root = node;
            this.resourceResolver = resourceResolver;
            this.tagWorkerFactory = new DefaultTagWorkerFactory();
        }

        public DefaultHtmlProcessor(INode node, ICssResolver cssResolver, ResourceResolver resourceResolver, ITagWorkerFactory
             tagWorkerFactory) {
            this.cssResolver = cssResolver;
            this.root = node;
            this.resourceResolver = resourceResolver;
            this.tagWorkerFactory = tagWorkerFactory;
        }

        public virtual IList<IElement> ProcessElements() {

            try 
            {
                String licenseKeyClassName = "iText.License.LicenseKey, itext.licensekey";
                String licenseKeyProductClassName = "iText.License.LicenseKeyProduct, itext.licensekey";
                String licenseKeyFeatureClassName = "iText.License.LicenseKeyProductFeature, itext.licensekey";
                String checkLicenseKeyMethodName = "ScheduledCheck";
                Type licenseKeyClass = GetClass(licenseKeyClassName);
                if ( licenseKeyClass != null ) 
                {                
                    Type licenseKeyProductClass = GetClass(licenseKeyProductClassName);
                    Type licenseKeyProductFeatureClass = GetClass(licenseKeyFeatureClassName);
                    Array array = Array.CreateInstance(licenseKeyProductFeatureClass, 0);
                    object[] objects = new object[] { "pdfHtml", 1, 0, array };
                    Object productObject = System.Activator.CreateInstance(licenseKeyProductClass, objects);
                    MethodInfo m = licenseKeyClass.GetMethod(checkLicenseKeyMethodName);
                    m.Invoke(System.Activator.CreateInstance(licenseKeyClass), new object[] {productObject});
                }   
            } 
            catch ( Exception e ) 
            {
                if ( !Kernel.Version.IsAGPLVersion() )
                {
                    throw;
                }
            }
            context = new ProcessorContext(cssResolver, resourceResolver);
            roots = new List<IPropertyContainer>();
            IElementNode html = FindHtmlNode(root);
            IElementNode body = FindBodyNode(root);
            // Force resolve styles to fetch default font size etc
            html.SetStyles(cssResolver.ResolveStyles(html));
            body.SetStyles(cssResolver.ResolveStyles(body));
            foreach (INode node in body.ChildNodes()) {
                if (node is IElementNode) {
                    Visit(node);
                }
                else {
                    if (node is ITextNode) {
                        logger.Error(String.Format(iText.Html2pdf.LogMessageConstant.TEXT_WAS_NOT_PROCESSED, ((ITextNode)node).WholeText
                            ()));
                    }
                }
            }
            context = null;
            IList<IElement> elements = new List<IElement>();
            foreach (IPropertyContainer propertyContainer in roots) {
                if (propertyContainer is IElement) {
                    propertyContainer.SetProperty(Property.COLLAPSING_MARGINS, true);
                    elements.Add((IElement)propertyContainer);
                }
            }
            roots = null;
            return elements;
        }

        private static Type GetClass(string className)
        {
            String licenseKeyClassFullName = null;
            Assembly assembly = typeof(DefaultHtmlProcessor).Assembly;
            Attribute keyVersionAttr = assembly.GetCustomAttribute(typeof(KeyVersionAttribute));
            if (keyVersionAttr is KeyVersionAttribute)
            {
                String keyVersion = ((KeyVersionAttribute)keyVersionAttr).KeyVersion;
                String format = "{0}, Version={1}, Culture=neutral, PublicKeyToken=8354ae6d2174ddca";
                licenseKeyClassFullName = String.Format(format, className, keyVersion);
            }
            Type type = null;
            if (licenseKeyClassFullName != null)
            {
                String fileLoadExceptionMessage = null;
                try
                {
                    type = System.Type.GetType(licenseKeyClassFullName);
                }
                catch (FileLoadException fileLoadException)
                {
                    fileLoadExceptionMessage = fileLoadException.Message;
                }
                if (fileLoadExceptionMessage != null)
                {
                    try
                    {
                        type = System.Type.GetType(className);
                    }
                    catch
                    {
                        // empty
                    }
                }
            }
            return type;
        }

        public virtual Document ProcessDocument(PdfDocument pdfDocument) {

            try 
            {
                String licenseKeyClassName = "iText.License.LicenseKey, itext.licensekey";
                String licenseKeyProductClassName = "iText.License.LicenseKeyProduct, itext.licensekey";
                String licenseKeyFeatureClassName = "iText.License.LicenseKeyProductFeature, itext.licensekey";
                String checkLicenseKeyMethodName = "ScheduledCheck";
                Type licenseKeyClass = GetClass(licenseKeyClassName);
                if ( licenseKeyClass != null ) 
                {                
                    Type licenseKeyProductClass = GetClass(licenseKeyProductClassName);
                    Type licenseKeyProductFeatureClass = GetClass(licenseKeyFeatureClassName);
                    Array array = Array.CreateInstance(licenseKeyProductFeatureClass, 0);
                    object[] objects = new object[] { "pdfHtml", 1, 0, array };
                    Object productObject = System.Activator.CreateInstance(licenseKeyProductClass, objects);
                    MethodInfo m = licenseKeyClass.GetMethod(checkLicenseKeyMethodName);
                    m.Invoke(System.Activator.CreateInstance(licenseKeyClass), new object[] {productObject});
                }   
            } 
            catch ( Exception e ) 
            {
                if ( !Kernel.Version.IsAGPLVersion() )
                {
                    throw;
                }
            }
            context = new ProcessorContext(cssResolver, pdfDocument, resourceResolver);
            // TODO store html version from document type in context if necessary
            roots = new List<IPropertyContainer>();
            root = FindHtmlNode(root);
            Visit(root);
            context = null;
            Document doc = (Document)roots[0];
            roots = null;
            return doc;
        }

        private void Visit(INode node) {
            if (node is IElementNode) {
                IElementNode element = (IElementNode)node;
                element.SetStyles(context.GetCssResolver().ResolveStyles(element));
                if (!IsDisplayable(element)) {
                    return;
                }
                //ITagWorker tagWorker = TagWorkerFactory.getTagWorker(element, context);
                ITagWorker tagWorker = tagWorkerFactory.GetTagWorkerInstance(element, context);
                if (tagWorker == null) {
                    // TODO for stylesheet links it looks ugly, but log errors will be printed for other <link> elements, not css links
                    if (!ignoredTags.Contains(element.Name()) && !HtmlUtils.IsStyleSheetLink(element)) {
                        logger.Error(String.Format(iText.Html2pdf.LogMessageConstant.NO_WORKER_FOUND_FOR_TAG, (element).Name()));
                    }
                }
                else {
                    context.GetState().Push(tagWorker);
                }
                foreach (INode childNode in element.ChildNodes()) {
                    Visit(childNode);
                }
                if (tagWorker != null) {
                    tagWorker.ProcessEnd(element, context);
                    context.GetState().Pop();
                    ICssApplier cssApplier = CssApplierFactory.GetCssApplier(element.Name());
                    if (cssApplier == null) {
                        if (!ignoredCssTags.Contains(element.Name())) {
                            logger.Error(String.Format(iText.Html2pdf.LogMessageConstant.NO_CSS_APPLIER_FOUND_FOR_TAG, element.Name())
                                );
                        }
                    }
                    else {
                        cssApplier.Apply(context, element, tagWorker);
                    }
                    if (!context.GetState().Empty()) {
                        bool childProcessed = context.GetState().Top().ProcessTagChild(tagWorker, context);
                        if (!childProcessed) {
                            logger.Error(String.Format(iText.Html2pdf.LogMessageConstant.WORKER_UNABLE_TO_PROCESS_OTHER_WORKER, context
                                .GetState().Top().GetType().FullName, tagWorker.GetType().FullName));
                        }
                    }
                    else {
                        if (tagWorker.GetElementResult() != null) {
                            roots.Add(tagWorker.GetElementResult());
                        }
                    }
                }
                element.SetStyles(null);
            }
            else {
                if (node is ITextNode) {
                    String content = ((ITextNode)node).WholeText();
                    if (content != null) {
                        if (!context.GetState().Empty()) {
                            bool contentProcessed = context.GetState().Top().ProcessContent(content, context);
                            if (!contentProcessed) {
                                logger.Error(String.Format(iText.Html2pdf.LogMessageConstant.WORKER_UNABLE_TO_PROCESS_IT_S_TEXT_CONTENT, context
                                    .GetState().Top().GetType().FullName));
                            }
                        }
                        else {
                            logger.Error(iText.Html2pdf.LogMessageConstant.NO_CONSUMER_FOUND_FOR_CONTENT);
                        }
                    }
                }
            }
        }

        private IElementNode FindElement(INode node, String tagName) {
            LinkedList<INode> q = new LinkedList<INode>();
            q.Add(node);
            while (!q.IsEmpty()) {
                INode currentNode = q.JGetFirst();
                q.RemoveFirst();
                if (currentNode is IElementNode && ((IElementNode)currentNode).Name().Equals(tagName)) {
                    return (IElementNode)currentNode;
                }
                foreach (INode child in currentNode.ChildNodes()) {
                    if (child is IElementNode) {
                        q.Add(child);
                    }
                }
            }
            return null;
        }

        private IElementNode FindHtmlNode(INode node) {
            return FindElement(node, TagConstants.HTML);
        }

        private IElementNode FindBodyNode(INode node) {
            return FindElement(node, TagConstants.BODY);
        }

        private static bool IsDisplayable(IElementNode element) {
            if (element != null && element.GetStyles() != null && CssConstants.NONE.Equals(element.GetStyles().Get(CssConstants
                .DISPLAY))) {
                return false;
            }
            return element != null;
        }
    }
}
