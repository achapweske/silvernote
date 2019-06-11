/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.TextFormatting;

namespace SilverNote.Editor
{
    public class GenericTextRunTypographyProperties : TextRunTypographyProperties, ICloneable
    {
        #region Static Members

        public static readonly GenericTextRunTypographyProperties Default = new GenericTextRunTypographyProperties();

        #endregion

        #region Fields

        int _AnnotationAlternates;
        FontCapitals _Capitals;
        bool _CapitalSpacing;
        bool _CaseSensitiveForms;
        bool _ContextualAlternates;
        bool _ContextualLigatures;
        int _ContextualSwashes;
        bool _DiscretionaryLigatures;
        bool _EastAsianExpertForms;
        FontEastAsianLanguage _EastAsianLanguage;
        FontEastAsianWidths _EastAsianWidths;
        FontFraction _Fraction;
        bool _HistoricalForms;
        bool _HistoricalLigatures;
        bool _Kerning;
        bool _MathematicalGreek;
        FontNumeralAlignment _NumeralAlignment;
        FontNumeralStyle _NumeralStyle;
        bool _SlashedZero;
        bool _StandardLigatures;
        int _StandardSwashes;
        int _StylisticAlternates;
        bool _StylisticSet1;
        bool _StylisticSet10;
        bool _StylisticSet11;
        bool _StylisticSet12;
        bool _StylisticSet13;
        bool _StylisticSet14;
        bool _StylisticSet15;
        bool _StylisticSet16;
        bool _StylisticSet17;
        bool _StylisticSet18;
        bool _StylisticSet19;
        bool _StylisticSet2;
        bool _StylisticSet20;
        bool _StylisticSet3;
        bool _StylisticSet4;
        bool _StylisticSet5;
        bool _StylisticSet6;
        bool _StylisticSet7;
        bool _StylisticSet8;
        bool _StylisticSet9;
        FontVariants _Variants;

        #endregion

        #region Constructors

        public GenericTextRunTypographyProperties()
        {
            _Capitals = FontCapitals.Normal;
            _ContextualAlternates = true;
            _ContextualLigatures = true;
            _EastAsianLanguage = FontEastAsianLanguage.Normal;
            _EastAsianWidths = FontEastAsianWidths.Normal;
            _Fraction = FontFraction.Normal;
            _Kerning = true;
            _NumeralAlignment = FontNumeralAlignment.Normal;
            _NumeralStyle = FontNumeralStyle.Normal;
            _StandardLigatures = true;
            _Variants = FontVariants.Normal;
        }

        public GenericTextRunTypographyProperties(TextRunTypographyProperties copy)
        {
            _AnnotationAlternates = copy.AnnotationAlternates;
            _Capitals = copy.Capitals;
            _CapitalSpacing = copy.CapitalSpacing;
            _CaseSensitiveForms = copy.CaseSensitiveForms;
            _ContextualAlternates = copy.ContextualAlternates;
            _ContextualLigatures = copy.ContextualLigatures;
            _ContextualSwashes = copy.ContextualSwashes;
            _DiscretionaryLigatures = copy.DiscretionaryLigatures;
            _EastAsianExpertForms = copy.EastAsianExpertForms;
            _EastAsianLanguage = copy.EastAsianLanguage;
            _EastAsianWidths = copy.EastAsianWidths;
            _Fraction = copy.Fraction;
            _HistoricalForms = copy.HistoricalForms;
            _HistoricalLigatures = copy.HistoricalLigatures;
            _Kerning = copy.Kerning;
            _MathematicalGreek = copy.MathematicalGreek;
            _NumeralAlignment = copy.NumeralAlignment;
            _NumeralStyle = copy.NumeralStyle;
            _SlashedZero = copy.SlashedZero;
            _StandardLigatures = copy.StandardLigatures;
            _StandardSwashes = copy.StandardSwashes;
            _StylisticAlternates = copy.StylisticAlternates;
            _StylisticSet1 = copy.StylisticSet1;
            _StylisticSet10 = copy.StylisticSet10;
            _StylisticSet11 = copy.StylisticSet11;
            _StylisticSet12 = copy.StylisticSet12;
            _StylisticSet13 = copy.StylisticSet13;
            _StylisticSet14 = copy.StylisticSet14;
            _StylisticSet15 = copy.StylisticSet15;
            _StylisticSet16 = copy.StylisticSet16;
            _StylisticSet17 = copy.StylisticSet17;
            _StylisticSet18 = copy.StylisticSet18;
            _StylisticSet19 = copy.StylisticSet19;
            _StylisticSet2 = copy.StylisticSet2;
            _StylisticSet20 = copy.StylisticSet20;
            _StylisticSet3 = copy.StylisticSet3;
            _StylisticSet4 = copy.StylisticSet4;
            _StylisticSet5 = copy.StylisticSet5;
            _StylisticSet6 = copy.StylisticSet6;
            _StylisticSet7 = copy.StylisticSet7;
            _StylisticSet8 = copy.StylisticSet8;
            _StylisticSet9 = copy.StylisticSet9;
            _Variants = copy.Variants;
        }

        #endregion

        #region Properties

        public override int AnnotationAlternates
        {
            get { return _AnnotationAlternates; }
        }

        public override FontCapitals Capitals
        {
            get { return _Capitals; }
        }

        public override bool CapitalSpacing
        {
            get { return _CapitalSpacing; }
        }
        
        public override bool CaseSensitiveForms
        {
            get { return _CaseSensitiveForms; }
        }

        public override bool ContextualAlternates
        {
            get { return _ContextualAlternates; }
        }

        public override bool ContextualLigatures
        {
            get { return _ContextualLigatures; }
        }

        public override int ContextualSwashes
        {
            get { return _ContextualSwashes; }
        }

        public override bool DiscretionaryLigatures
        {
            get { return _DiscretionaryLigatures; }
        }

        public override bool EastAsianExpertForms
        {
            get { return _EastAsianExpertForms; }
        }

        public override FontEastAsianLanguage EastAsianLanguage
        {
            get { return _EastAsianLanguage; }
        }

        public override FontEastAsianWidths EastAsianWidths
        {
            get { return _EastAsianWidths; }
        }

        public override FontFraction Fraction
        {
            get { return _Fraction; }
        }

        public override bool HistoricalForms
        {
            get { return _HistoricalForms; }
        }

        public override bool HistoricalLigatures
        {
            get { return _HistoricalLigatures; }
        }

        public override bool Kerning
        {
            get { return _Kerning; }
        }

        public override bool MathematicalGreek
        {
            get { return _MathematicalGreek; }
        }

        public override FontNumeralAlignment NumeralAlignment
        {
            get { return _NumeralAlignment; }
        }

        public override FontNumeralStyle NumeralStyle
        {
            get { return _NumeralStyle; }
        }

        public override bool SlashedZero
        {
            get { return _SlashedZero; }
        }

        public override bool StandardLigatures
        {
            get { return _StandardLigatures; }
        }

        public override int StandardSwashes
        {
            get { return _StandardSwashes; }
        }

        public override int StylisticAlternates
        {
            get { return _StylisticAlternates; }
        }

        public override bool StylisticSet1
        {
            get { return _StylisticSet1; }
        }

        public override bool StylisticSet10
        {
            get { return _StylisticSet10; }
        }

        public override bool StylisticSet11
        {
            get { return _StylisticSet11; }
        }

        public override bool StylisticSet12
        {
            get { return _StylisticSet12; }
        }

        public override bool StylisticSet13
        {
            get { return _StylisticSet13; }
        }

        public override bool StylisticSet14
        {
            get { return _StylisticSet14; }
        }

        public override bool StylisticSet15
        {
            get { return _StylisticSet15; }
        }

        public override bool StylisticSet16
        {
            get { return _StylisticSet16; }
        }

        public override bool StylisticSet17
        {
            get { return _StylisticSet17; }
        }

        public override bool StylisticSet18
        {
            get { return _StylisticSet18; }
        }

        public override bool StylisticSet19
        {
            get { return _StylisticSet19; }
        }

        public override bool StylisticSet2
        {
            get { return _StylisticSet2; }
        }

        public override bool StylisticSet20
        {
            get { return _StylisticSet20; }
        }

        public override bool StylisticSet3
        {
            get { return _StylisticSet3; }
        }

        public override bool StylisticSet4
        {
            get { return _StylisticSet4; }
        }

        public override bool StylisticSet5
        {
            get { return _StylisticSet5; }
        }

        public override bool StylisticSet6
        {
            get { return _StylisticSet6; }
        }

        public override bool StylisticSet7
        {
            get { return _StylisticSet7; }
        }

        public override bool StylisticSet8
        {
            get { return _StylisticSet8; }
        }

        public override bool StylisticSet9
        {
            get { return _StylisticSet9; }
        }

        public override FontVariants Variants
        {
            get { return _Variants; }
        }

        #endregion

        #region Methods

        public void SetAnnotationAlternates(int annotationAlternates)
        {
            if (annotationAlternates != _AnnotationAlternates)
            {
                _AnnotationAlternates = annotationAlternates;
                OnPropertiesChanged();
            }
        }

        public void SetCapitals(FontCapitals capitals)
        {
            if (capitals != _Capitals)
            {
                _Capitals = capitals;
                OnPropertiesChanged();
            }
        }
        public void SetCapitalSpacing(bool capitalSpacing)
        {
            if (capitalSpacing != _CapitalSpacing)
            {
                _CapitalSpacing = capitalSpacing;
                OnPropertiesChanged();
            }
        }
        public void SetCaseSensitiveForms(bool caseSensitiveForms)
        {
            if (caseSensitiveForms != _CaseSensitiveForms)
            {
                _CaseSensitiveForms = caseSensitiveForms;
                OnPropertiesChanged();
            }
        }
        public void SetContextualAlternates(bool contextualAlternates)
        {
            if (contextualAlternates != _ContextualAlternates)
            {
                _ContextualAlternates = contextualAlternates;
                OnPropertiesChanged();
            }
        }
        public void SetContextualLigatures(bool contextualLigatures)
        {
            if (contextualLigatures != _ContextualLigatures)
            {
                _ContextualLigatures = contextualLigatures;
                OnPropertiesChanged();
            }
        }
        public void SetContextualSwashes(int contextualSwashes)
        {
            if (contextualSwashes != _ContextualSwashes)
            {
                _ContextualSwashes = contextualSwashes;
                OnPropertiesChanged();
            }
        }
        public void SetDiscretionaryLigatures(bool discretionaryLigatures)
        {
            if (discretionaryLigatures != _DiscretionaryLigatures)
            {
                _DiscretionaryLigatures = discretionaryLigatures;
                OnPropertiesChanged();
            }
        }
        public void SetEastAsianExpertForms(bool eastAsianExpertForms)
        {
            if (eastAsianExpertForms != _EastAsianExpertForms)
            {
                _EastAsianExpertForms = eastAsianExpertForms;
                OnPropertiesChanged();
            }
        }
        public void SetEastAsianLanguage(FontEastAsianLanguage eastAsianLanguage)
        {
            if (eastAsianLanguage != _EastAsianLanguage)
            {
                _EastAsianLanguage = eastAsianLanguage;
                OnPropertiesChanged();
            }
        }
        public void SetEastAsianWidths(FontEastAsianWidths eastAsianWidths)
        {
            if (eastAsianWidths != _EastAsianWidths)
            {
                _EastAsianWidths = eastAsianWidths;
                OnPropertiesChanged();
            }
        }
        public void SetFraction(FontFraction fraction)
        {
            if (fraction != _Fraction)
            {
                _Fraction = fraction;
                OnPropertiesChanged();
            }
        }
        public void SetHistoricalForms(bool historicalForms)
        {
            if (historicalForms != _HistoricalForms)
            {
                _HistoricalForms = historicalForms;
                OnPropertiesChanged();
            }
        }
        public void SetHistoricalLigatures(bool historicalLigatures)
        {
            if (historicalLigatures != _HistoricalLigatures)
            {
                _HistoricalLigatures = historicalLigatures;
                OnPropertiesChanged();
            }
        }
        public void SetKerning(bool kerning)
        {
            if (kerning != _Kerning)
            {
                _Kerning = kerning;
                OnPropertiesChanged();
            }
        }
        public void SetMathematicalGreek(bool mathematicalGreek)
        {
            if (mathematicalGreek != _MathematicalGreek)
            {
                _MathematicalGreek = mathematicalGreek;
                OnPropertiesChanged();
            }
        }
        public void SetNumeralAlignment(FontNumeralAlignment numeralAlignment)
        {
            if (numeralAlignment != _NumeralAlignment)
            {
                _NumeralAlignment = numeralAlignment;
                OnPropertiesChanged();
            }
        }
        public void SetNumeralStyle(FontNumeralStyle numeralStyle)
        {
            if (numeralStyle != _NumeralStyle)
            {
                _NumeralStyle = numeralStyle;
                OnPropertiesChanged();
            }
        }
        public void SetSlashedZero(bool slashedZero)
        {
            if (slashedZero != _SlashedZero)
            {
                _SlashedZero = slashedZero;
                OnPropertiesChanged();
            }
        }
        public void SetStandardLigatures(bool standardLigatures)
        {
            if (standardLigatures != _StandardLigatures)
            {
                _StandardLigatures = standardLigatures;
                OnPropertiesChanged();
            }
        }
        public void SetStandardSwashes(int standardSwashes)
        {
            if (standardSwashes != _StandardSwashes)
            {
                _StandardSwashes = standardSwashes;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticAlternates(int stylisticAlternates)
        {
            if (stylisticAlternates != _StylisticAlternates)
            {
                _StylisticAlternates = stylisticAlternates;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet1(bool stylisticSet1)
        {
            if (stylisticSet1 != _StylisticSet1)
            {
                _StylisticSet1 = stylisticSet1;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet10(bool stylisticSet10)
        {
            if (stylisticSet10 != _StylisticSet10)
            {
                _StylisticSet10 = stylisticSet10;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet11(bool stylisticSet11)
        {
            if (stylisticSet11 != _StylisticSet11)
            {
                _StylisticSet11 = stylisticSet11;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet12(bool stylisticSet12)
        {
            if (stylisticSet12 != _StylisticSet12)
            {
                _StylisticSet12 = stylisticSet12;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet13(bool stylisticSet13)
        {
            if (stylisticSet13 != _StylisticSet13)
            {
                _StylisticSet13 = stylisticSet13;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet14(bool stylisticSet14)
        {
            if (stylisticSet14 != _StylisticSet14)
            {
                _StylisticSet14 = stylisticSet14;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet15(bool stylisticSet15)
        {
            if (stylisticSet15 != _StylisticSet15)
            {
                _StylisticSet15 = stylisticSet15;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet16(bool stylisticSet16)
        {
            if (stylisticSet16 != _StylisticSet16)
            {
                _StylisticSet16 = stylisticSet16;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet17(bool stylisticSet17)
        {
            if (stylisticSet17 != _StylisticSet17)
            {
                _StylisticSet17 = stylisticSet17;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet18(bool stylisticSet18)
        {
            if (stylisticSet18 != _StylisticSet18)
            {
                _StylisticSet18 = stylisticSet18;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet19(bool stylisticSet19)
        {
            if (stylisticSet19 != _StylisticSet19)
            {
                _StylisticSet19 = stylisticSet19;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet2(bool stylisticSet2)
        {
            if (stylisticSet2 != _StylisticSet2)
            {
                _StylisticSet2 = stylisticSet2;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet20(bool stylisticSet20)
        {
            if (stylisticSet20 != _StylisticSet20)
            {
                _StylisticSet20 = stylisticSet20;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet3(bool stylisticSet3)
        {
            if (stylisticSet3 != _StylisticSet3)
            {
                _StylisticSet3 = stylisticSet3;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet4(bool stylisticSet4)
        {
            if (stylisticSet4 != _StylisticSet4)
            {
                _StylisticSet4 = stylisticSet4;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet5(bool stylisticSet5)
        {
            if (stylisticSet5 != _StylisticSet5)
            {
                _StylisticSet5 = stylisticSet5;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet6(bool stylisticSet6)
        {
            if (stylisticSet6 != _StylisticSet6)
            {
                _StylisticSet6 = stylisticSet6;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet7(bool stylisticSet7)
        {
            if (stylisticSet7 != _StylisticSet7)
            {
                _StylisticSet7 = stylisticSet7;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet8(bool stylisticSet8)
        {
            if (stylisticSet8 != _StylisticSet8)
            {
                _StylisticSet8 = stylisticSet8;
                OnPropertiesChanged();
            }
        }
        public void SetStylisticSet9(bool stylisticSet9)
        {
            if (stylisticSet9 != _StylisticSet9)
            {
                _StylisticSet9 = stylisticSet9;
                OnPropertiesChanged();
            }
        }


        public void SetVariants(FontVariants variants)
        {
            if (variants != _Variants)
            {
                _Variants = variants;
                OnPropertiesChanged();
            }
        }

        #endregion

        #region ICloneable

        public virtual object Clone()
        {
            return new GenericTextRunTypographyProperties(this);
        }

        #endregion
    }
}
