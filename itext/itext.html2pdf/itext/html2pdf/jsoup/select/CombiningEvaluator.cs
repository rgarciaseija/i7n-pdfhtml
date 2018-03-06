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
using System.Collections.Generic;
using iText.IO.Util;

namespace iText.Html2pdf.Jsoup.Select {
    /// <summary>Base combining (and, or) evaluator.</summary>
    internal abstract class CombiningEvaluator : Evaluator {
        internal readonly List<Evaluator> evaluators;

        internal int num = 0;

        internal CombiningEvaluator()
            : base() {
            evaluators = new List<Evaluator>();
        }

        internal CombiningEvaluator(ICollection<Evaluator> evaluators)
            : this() {
            this.evaluators.AddAll(evaluators);
            UpdateNumEvaluators();
        }

        internal virtual Evaluator RightMostEvaluator() {
            return num > 0 ? evaluators[num - 1] : null;
        }

        internal virtual void ReplaceRightMostEvaluator(Evaluator replacement) {
            evaluators[num - 1] = replacement;
        }

        internal virtual void UpdateNumEvaluators() {
            // used so we don't need to bash on size() for every match test
            num = evaluators.Count;
        }

        internal sealed class And : CombiningEvaluator {
            internal And(ICollection<Evaluator> evaluators)
                : base(evaluators) {
            }

            internal And(params Evaluator[] evaluators)
                : this(JavaUtil.ArraysAsList(evaluators)) {
            }

            public override bool Matches(iText.Html2pdf.Jsoup.Nodes.Element root, iText.Html2pdf.Jsoup.Nodes.Element node
                ) {
                for (int i = 0; i < num; i++) {
                    Evaluator s = evaluators[i];
                    if (!s.Matches(root, node)) {
                        return false;
                    }
                }
                return true;
            }

            public override String ToString() {
                return iText.Html2pdf.Jsoup.Helper.StringUtil.Join(evaluators, " ");
            }
        }

        internal sealed class OR : CombiningEvaluator {
            /// <summary>Create a new Or evaluator.</summary>
            /// <remarks>Create a new Or evaluator. The initial evaluators are ANDed together and used as the first clause of the OR.
            ///     </remarks>
            /// <param name="evaluators">initial OR clause (these are wrapped into an AND evaluator).</param>
            internal OR(ICollection<Evaluator> evaluators)
                : base() {
                if (num > 1) {
                    this.evaluators.Add(new CombiningEvaluator.And(evaluators));
                }
                else {
                    // 0 or 1
                    this.evaluators.AddAll(evaluators);
                }
                UpdateNumEvaluators();
            }

            internal OR()
                : base() {
            }

            public void Add(Evaluator e) {
                evaluators.Add(e);
                UpdateNumEvaluators();
            }

            public override bool Matches(iText.Html2pdf.Jsoup.Nodes.Element root, iText.Html2pdf.Jsoup.Nodes.Element node
                ) {
                for (int i = 0; i < num; i++) {
                    Evaluator s = evaluators[i];
                    if (s.Matches(root, node)) {
                        return true;
                    }
                }
                return false;
            }

            public override String ToString() {
                return MessageFormatUtil.Format(":or{0}", evaluators);
            }
        }
    }
}