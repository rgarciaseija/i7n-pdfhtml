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
address: sales@itextpdf.com
*/
using System;
using System.IO;
using iText.Html2pdf;
using iText.IO.Util;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Test;

namespace iText.Html2pdf.Css {
    public class CssOutlineTest : ExtendedITextTest {
        public static readonly String sourceFolder = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/html2pdf/css/CssOutlineTest/";

        public static readonly String destinationFolder = NUnit.Framework.TestContext.CurrentContext.TestDirectory
             + "/test/itext/html2pdf/css/CssOutlineTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void BeforeClass() {
            CreateDestinationFolder(destinationFolder);
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CssOutlineTest01() {
            HtmlConverter.ConvertToPdf(new FileInfo(sourceFolder + "cssOutline01.html"), new FileInfo(destinationFolder
                 + "cssOutline01.pdf"));
            System.Console.Out.WriteLine("html: file:///" + UrlUtil.ToNormalizedURI(sourceFolder + "cssOutline01.html"
                ).AbsolutePath + "\n");
            NUnit.Framework.Assert.IsNull(new CompareTool().CompareByContent(destinationFolder + "cssOutline01.pdf", sourceFolder
                 + "cmp_cssOutline01.pdf", destinationFolder, "diff01_"));
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CssOutlineTest02() {
            HtmlConverter.ConvertToPdf(new FileInfo(sourceFolder + "cssOutline02.html"), new FileInfo(destinationFolder
                 + "cssOutline02.pdf"));
            System.Console.Out.WriteLine("html: file:///" + UrlUtil.ToNormalizedURI(sourceFolder + "cssOutline02.html"
                ).AbsolutePath + "\n");
            NUnit.Framework.Assert.IsNull(new CompareTool().CompareByContent(destinationFolder + "cssOutline02.pdf", sourceFolder
                 + "cmp_cssOutline02.pdf", destinationFolder, "diff02_"));
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CssOutlineTest03() {
            HtmlConverter.ConvertToPdf(new FileInfo(sourceFolder + "cssOutline03.html"), new FileInfo(destinationFolder
                 + "cssOutline03.pdf"));
            System.Console.Out.WriteLine("html: file:///" + UrlUtil.ToNormalizedURI(sourceFolder + "cssOutline03.html"
                ).AbsolutePath + "\n");
            NUnit.Framework.Assert.IsNull(new CompareTool().CompareByContent(destinationFolder + "cssOutline03.pdf", sourceFolder
                 + "cmp_cssOutline03.pdf", destinationFolder, "diff03_"));
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CssOutlineTest04() {
            PdfDocument outDoc = new PdfDocument(new PdfWriter(destinationFolder + "cssOutline04.pdf"));
            outDoc.SetTagged();
            using (FileStream fileInputStream = new FileStream(sourceFolder + "cssOutline04.html", FileMode.Open, FileAccess.Read
                )) {
                HtmlConverter.ConvertToPdf(fileInputStream, outDoc);
            }
            System.Console.Out.WriteLine("html: file:///" + UrlUtil.ToNormalizedURI(sourceFolder + "cssOutline04.html"
                ).AbsolutePath + "\n");
            NUnit.Framework.Assert.IsNull(new CompareTool().CompareByContent(destinationFolder + "cssOutline04.pdf", sourceFolder
                 + "cmp_cssOutline04.pdf", destinationFolder, "diff04_"));
        }

        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void CssOutlineTest05() {
            HtmlConverter.ConvertToPdf(new FileInfo(sourceFolder + "cssOutline05.html"), new FileInfo(destinationFolder
                 + "cssOutline05.pdf"));
            System.Console.Out.WriteLine("html: file:///" + UrlUtil.ToNormalizedURI(sourceFolder + "cssOutline05.html"
                ).AbsolutePath + "\n");
            NUnit.Framework.Assert.IsNull(new CompareTool().CompareByContent(destinationFolder + "cssOutline05.pdf", sourceFolder
                 + "cmp_cssOutline05.pdf", destinationFolder, "diff05_"));
        }
    }
}