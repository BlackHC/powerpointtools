﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.PowerPoint;
using System.Diagnostics;

namespace PowerPointLaTeX.InlineFormulas
{
    static class Embedding
    {
        /// <summary>
        /// Compile latexCode into an inline shape
        /// </summary>
        /// <param name="slide"></param>
        /// <param name="textShape"></param>
        /// <param name="latexCode"></param>
        /// <param name="codeRange"></param>
        /// <returns></returns>
        static private Shape CompileInlineLaTeXCode(Slide slide, Shape textShape, string latexCode, TextRange codeRange)
        {
            Shape picture = LaTeXRendering.GetPictureShapeFromLaTeXCode(slide, latexCode, codeRange.Font.Size);
            if (picture == null)
            {
                return null;
            }

            // add tags to the picture
            picture.LaTeXTags().Code.value = latexCode;
            picture.LaTeXTags().Type.value = EquationType.Inline;
            picture.LaTeXTags().LinkID.value = textShape.Id;

            InlineFormulas.Alignment.PrepareTextRange(picture, codeRange);

            CopyInlineEffects(slide, textShape, codeRange, picture);

            return picture;
        }

        static private void CopyInlineEffects(Slide slide, Shape textShape, TextRange codeRange, Shape picture)
        {
            try
            {
                // copy animations from the parent textShape
                Sequence sequence = slide.TimeLine.MainSequence;
                var effects =
                    from Effect effect in sequence
                    where effect.Shape.SafeThis() != null && effect.Shape == textShape &&
                    ((effect.EffectInformation.TextUnitEffect == MsoAnimTextUnitEffect.msoAnimTextUnitEffectByParagraph &&
                        textShape.ParagraphContainsRange(effect.GetSafeParagraph(), codeRange))
                        || effect.EffectInformation.BuildByLevelEffect == MsoAnimateByLevel.msoAnimateLevelNone)
                    select effect;

                picture.AddEffects(effects, true, sequence);
            }
            catch
            {
                Debug.Fail("CopyInlineEffects failed!");
            }
        }

        static private void CompileInlineTextRange(Slide slide, Shape shape, TextRange range)
        {
            int startIndex = 0;

            int codeCount = 0;

            List<TextRange> pictureRanges = new List<TextRange>();
            List<Shape> pictures = new List<Shape>();

            while (true)
            {
                bool inlineMode;
                int latexCodeStartIndex;
                int latexCodeEndIndex;
                int endIndex;

                int inlineStartIndex, displaystyleStartIndex;
                inlineStartIndex = range.Text.IndexOf("$$", startIndex);
                displaystyleStartIndex = range.Text.IndexOf("$$[", startIndex);
                if (displaystyleStartIndex != -1 && displaystyleStartIndex <= inlineStartIndex)
                {
                    inlineMode = false;

                    startIndex = displaystyleStartIndex;
                    latexCodeStartIndex = startIndex + 3;
                    latexCodeEndIndex = range.Text.IndexOf("]$$", latexCodeStartIndex);
                    endIndex = latexCodeEndIndex + 3;
                }
                else if (inlineStartIndex != -1)
                {
                    inlineMode = true;

                    startIndex = inlineStartIndex;
                    latexCodeStartIndex = startIndex + 2;
                    latexCodeEndIndex = range.Text.IndexOf("$$", latexCodeStartIndex);
                    endIndex = latexCodeEndIndex + 2;
                }
                else
                {
                    break;
                }

                if (latexCodeEndIndex == -1)
                {
                    break;
                }

                int length = endIndex - startIndex;

                int latexCodeLength = latexCodeEndIndex - latexCodeStartIndex;
                string latexCode = range.Text.Substring(latexCodeStartIndex, latexCodeLength);
                latexCode = TransformSpecialUnicodeCharactersToAscii(latexCode);

                // must be [[ then
                if (!inlineMode)
                {
                    latexCode = @"\displaystyle{" + latexCode + "}";
                }

                LaTeXEntry tagEntry = shape.LaTeXTags().Entries[codeCount];
                tagEntry.Code.value = latexCode;
                // TODO: cohesion? [5/2/2009 Andreas]
                // save the font size because it might be changed later
                // +1 because IndexOf is base 0, but Characters uses base 1
                tagEntry.FontSize.value = range.Characters(latexCodeStartIndex + 1, latexCodeLength).Font.Size;

                // escape $$!$$
                // +1 because IndexOf is base 0, but Characters uses base 1
                TextRange codeRange = range.Characters(startIndex + 1, length);
                if (!Helpers.IsEscapeCode(latexCode))
                {
                    Shape picture = CompileInlineLaTeXCode(slide, shape, latexCode, codeRange);
                    if (picture != null)
                    {
                        tagEntry.ShapeId.value = picture.Id;

                        pictures.Add(picture);
                        pictureRanges.Add(codeRange);
                    }
                    else
                    {
                        codeRange.Text = "$Formula Error$";
                    }
                }
                else
                {
                    codeRange.Text = "$$";
                }

                tagEntry.StartIndex.value = codeRange.Start;
                tagEntry.Length.value = codeRange.Length;

                // NOTE: endIndex isnt valid anymore since we've removed some text [5/24/2010 Andreas
                // IndexOf uses base0, codeRange base1 => -1
                startIndex = codeRange.Start + codeRange.Length - 1;
                codeCount++;
            }

            // now that everything has been converted we can position the formulas (pictures) in the text area
            for (int i = 0; i < pictures.Count; i++)
            {
                TextRange codeRange = pictureRanges[i];
                InlineFormulas.Alignment.Align(codeRange, pictures[i]);
            }

            // update the type, too
            shape.LaTeXTags().Type.value = codeCount > 0 ? EquationType.HasCompiledInlines : EquationType.None;
        }

        static private string TransformSpecialUnicodeCharactersToAscii(string text)
        {
            text = text.Replace((char)8217, '\'');
            // replace weird unicode - (hypens) with minus
            text = text.Replace((char)8208, '-');
            text = text.Replace((char)8211, '-');
            text = text.Replace((char)8212, '-');
            text = text.Replace((char)8722, '-');
            text = text.Replace((char)8209, '-');
            text = text.Replace((char)8259, '-');
            // replace ellipses with ...
            text = text.Replace(((char)8230).ToString(), "...");

            return text;
        }

        static private void DecompileTextRange(Slide slide, Shape shape, TextRange range)
        {
            // make sure this is always valid, otherwise the code will do stupid things
            Debug.Assert(shape.LaTeXTags().Type == EquationType.HasCompiledInlines);

            LaTeXEntries entries = shape.LaTeXTags().Entries;
            int length = entries.Length;
            for (int i = length - 1; i >= 0; i--)
            {
                LaTeXEntry entry = entries[i];
                string latexCode = entry.Code;

                if (!Helpers.IsEscapeCode(latexCode))
                {
                    int shapeID = entry.ShapeId;
                    // find the shape
                    Shape picture = slide.Shapes.FindById(shapeID);

                    //Debug.Assert(picture != null);
                    // fail gracefully
                    if (picture != null)
                    {
                        Debug.Assert(picture.LaTeXTags().Type == EquationType.Inline);
                        picture.Delete();
                    }
                }

                // add back the latex code
                TextRange codeRange = range.Characters(entry.StartIndex, entry.Length);
                if (latexCode.StartsWith(@"\displaystyle{") && latexCode.EndsWith("}"))
                {
                    codeRange.Text = "$$[" + latexCode.Substring(@"\displayStyle{".Length, latexCode.Length - 1 - @"\displayStyle{".Length) + "]$$";
                }
                else
                {
                    codeRange.Text = "$$" + latexCode + "$$";
                }
                if (entry.FontSize != 0)
                {
                    codeRange.Font.Size = entry.FontSize;
                }
                codeRange.Font.BaselineOffset = 0.0f;
            }

            entries.Clear();
            shape.LaTeXTags().Type.value = EquationType.HasInlines;
        }

        static public void CompileShape(Slide slide, Shape shape)
        {
            // we don't need to compile already compiled shapes (its also sensible to avoid destroying escape sequences or overwrite entries, etc)
            // don't try to compile equations (or their sources) either
            EquationType type = shape.LaTeXTags().Type;
            if (type == EquationType.HasCompiledInlines || type == EquationType.Equation)
            {
                return;
            }

            if (shape.HasTextFrame == Microsoft.Office.Core.MsoTriState.msoTrue)
            {
                TextFrame textFrame = shape.TextFrame;
                if (textFrame.HasText == Microsoft.Office.Core.MsoTriState.msoTrue)
                {
                    CompileInlineTextRange(slide, shape, textFrame.TextRange);
                }
            }
        }

        static public void DecompileShape(Slide slide, Shape shape)
        {
            // we don't need to decompile already shapes that aren't compiled
            if (shape.LaTeXTags().Type != EquationType.HasCompiledInlines)
            {
                return;
            }

            if (shape.HasTextFrame == Microsoft.Office.Core.MsoTriState.msoTrue)
            {
                TextFrame textFrame = shape.TextFrame;
                if (textFrame.HasText == Microsoft.Office.Core.MsoTriState.msoTrue)
                {
                    DecompileTextRange(slide, shape, textFrame.TextRange);
                }
            }
        }
    }
}
