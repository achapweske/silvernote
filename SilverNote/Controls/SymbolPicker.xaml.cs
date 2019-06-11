/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SilverNote.Controls
{
    public delegate void SymbolPickedEventHandler(object sender, SymbolPickedEventArgs e);

    public class SymbolPickedEventArgs : RoutedEventArgs
    {
        public SymbolPickedEventArgs(RoutedEvent routedEvent, char symbol)
            : base(routedEvent)
        {
            Symbol = symbol;
        }

        public char Symbol { get; set; }
    }

    public partial class SymbolPicker : UserControl
    {
        public class Category
        {
            public Category(string name = null)
            {
                Name = name;
                Entries = new ObservableCollection<Entry>();
            }

            public string Name { get; set; }

            public ObservableCollection<Entry> Entries { get; private set; }
        }

        public class Entry
        {
            public Entry()
            { }

            public Entry(string name, char value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; set; }

            public char Value { get; set; }
        }

        public SymbolPicker()
        {
            Categories = new ObservableCollection<Category>();

            Categories.Add(Arrows);
            Categories.Add(Currency);
            Categories.Add(Fractions);
            Categories.Add(GreekLowercase);
            Categories.Add(GreekUppercase);
            Categories.Add(Legal);
            Categories.Add(LogicAndSetTheory);
            Categories.Add(Math);
            Categories.Add(Punctuation);
            Categories.Add(Typography);

            InitializeComponent();
        }

        public ObservableCollection<Category> Categories { get; private set; }

        #region Command

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(RoutedUICommand),
            typeof(SymbolPicker)
        );

        public RoutedUICommand Command
        {
            get { return (RoutedUICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        #endregion

        #region CommandTarget

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
            "CommandTarget",
            typeof(IInputElement),
            typeof(SymbolPicker)
        );

        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        #endregion

        private void Symbol_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item != null)
            {
                Entry entry = item.Content as Entry;
                if (entry != null)
                {
                    if (Command != null)
                    {
                        string symbol = new string(new char[] { entry.Value });
                        Command.Execute(symbol, CommandTarget);

                        if (CommandTarget != null)
                        {
                            CommandTarget.Focus();
                        }
                    }
                }
            }
        }

        #region Arrows

        private static Category arrows = null;

        public Category Arrows
        {
            get
            {
                if (arrows == null)
                {
                    arrows = new Category("Arrows");

                    Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        LoadArrows();
                    }));
                }

                return arrows;
            }
        }

        public static void LoadArrows()
        {
            if (arrows == null)
            {
                arrows = new Category("Arrows");
            }

            if (arrows.Entries.Count == 0)
            {
                Category category = arrows;

                // Simple arrows
                category.Entries.Add(new Entry("Left arrow", '\u2190'));
                category.Entries.Add(new Entry("Up arrow", '\u2191'));
                category.Entries.Add(new Entry("Right arrow", '\u2192'));
                category.Entries.Add(new Entry("Down arrow", '\u2193'));
                category.Entries.Add(new Entry("Left-right arrow", '\u2194'));
                category.Entries.Add(new Entry("Up-down arrow", '\u2195'));
                //category.Entries.Add(new Entry("Northwest arrow", '\u2196'));
                //category.Entries.Add(new Entry("Northeast arrow", '\u2197'));
                //category.Entries.Add(new Entry("Southeast arrow", '\u2198'));
                //category.Entries.Add(new Entry("Southwest arrow", '\u2199'));

                // Dashed arrows
                category.Entries.Add(new Entry("Left dashed arrow", '\u21E0'));
                category.Entries.Add(new Entry("Up dashed arrow", '\u21E1'));
                category.Entries.Add(new Entry("Right dashed arrow", '\u21E2'));
                category.Entries.Add(new Entry("Down dashed arrow", '\u21E3'));

                // Double arrows
                category.Entries.Add(new Entry("Left double arrow", '\u21D0'));
                category.Entries.Add(new Entry("Up double arrow", '\u21D1'));
                category.Entries.Add(new Entry("Right double arrow", '\u21D2'));
                category.Entries.Add(new Entry("Down double arrow", '\u21D3'));
                category.Entries.Add(new Entry("Left-right double arrow", '\u21D4'));
                category.Entries.Add(new Entry("Up-down double arrow", '\u21D5'));
                //category.Entries.Add(new Entry("Northwest double arrow", '\u21D6'));
                //category.Entries.Add(new Entry("Northeast double arrow", '\u21D7'));
                //category.Entries.Add(new Entry("Southeast double arrow", '\u21D8'));
                //category.Entries.Add(new Entry("Southwest double arrow", '\u21D9'));

                // White arrows
                category.Entries.Add(new Entry("Left white arrow", '\u21E6'));
                category.Entries.Add(new Entry("Up white arrow", '\u21E7'));
                category.Entries.Add(new Entry("Right white arrow", '\u21E8'));
                category.Entries.Add(new Entry("Down white arrow", '\u21E9'));


                // Harpoons
                category.Entries.Add(new Entry("Left harpoon, barb up", '\u21BC'));
                category.Entries.Add(new Entry("Left harpoon, barb down", '\u21BD'));
                category.Entries.Add(new Entry("Up harpoon, barb right", '\u21BE'));
                category.Entries.Add(new Entry("Up harpoon, barb left", '\u21BF'));
                category.Entries.Add(new Entry("Right harpoon, barb up", '\u21C0'));
                category.Entries.Add(new Entry("Right harpoon, barb down", '\u21C1'));
                category.Entries.Add(new Entry("Down harpoon, barb right", '\u21C2'));
                category.Entries.Add(new Entry("Down harpoon, barb left", '\u21C3'));

                // Paired arrows
                category.Entries.Add(new Entry("Right-left arrows", '\u21C4'));
                category.Entries.Add(new Entry("Left-right arrows", '\u21C6'));
                category.Entries.Add(new Entry("Up-down arrows", '\u21C5'));
                //category.Entries.Add(new Entry("Down-up arrows", '\u21F5'));
                category.Entries.Add(new Entry("Left paired arrows", '\u21C7'));
                category.Entries.Add(new Entry("Up paired arrows", '\u21C8'));
                category.Entries.Add(new Entry("Right paired arrows", '\u21C9'));
                category.Entries.Add(new Entry("Down paired arrows", '\u21CA'));
                category.Entries.Add(new Entry("Left over right harpoon", '\u21CB'));
                category.Entries.Add(new Entry("Right over left harpoon", '\u21CC'));



            }
        }

        #endregion

        #region Currency

        private static Category currency = null;

        public Category Currency
        {
            get
            {
                if (currency == null)
                {
                    currency = new Category("Currency");

                    Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        LoadCurrency();
                    }));
                }

                return currency;
            }
        }

        public static void LoadCurrency()
        {
            if (currency == null)
            {
                currency = new Category("Currency");
            }

            if (currency.Entries.Count == 0)
            {
                Category category = currency;

                category.Entries.Add(new Entry("Currency", '\u00A4'));
                category.Entries.Add(new Entry("Dollar", '\u0024'));
                category.Entries.Add(new Entry("Cent", '\u00A2'));
                category.Entries.Add(new Entry("Euro", '\u20AC'));
                category.Entries.Add(new Entry("Pound", '\u00A3'));
                category.Entries.Add(new Entry("Yen/Yuan", '\u00A5'));
                category.Entries.Add(new Entry("Won", '\u20A9'));
                category.Entries.Add(new Entry("Rupee", '\u20A8'));

                category.Entries.Add(new Entry("ECU", '\u20A0'));
                category.Entries.Add(new Entry("Colon", '\u20A1'));
                category.Entries.Add(new Entry("Cruzeir", '\u20A2'));
                category.Entries.Add(new Entry("Franc", '\u20A3'));
                category.Entries.Add(new Entry("Lira", '\u20A4'));
                category.Entries.Add(new Entry("Mill", '\u20A5'));
                category.Entries.Add(new Entry("Naira", '\u20A6'));
                category.Entries.Add(new Entry("Peseta", '\u20A7'));
                category.Entries.Add(new Entry("Sheqel", '\u20AA'));
                category.Entries.Add(new Entry("Dong", '\u20AB'));
                category.Entries.Add(new Entry("Kip", '\u20AD'));
                category.Entries.Add(new Entry("Tugrik", '\u20AE'));
                category.Entries.Add(new Entry("Drachma", '\u20AF'));
                category.Entries.Add(new Entry("German penny", '\u20B0'));
                category.Entries.Add(new Entry("Peso", '\u20B1'));

                category.Entries.Add(new Entry("Afghani", '\u060B'));
                category.Entries.Add(new Entry("Baht", '\u0E3F'));
                category.Entries.Add(new Entry("Guilder", '\u0192'));
                category.Entries.Add(new Entry("Lempira", '\u004C'));
                category.Entries.Add(new Entry("Quetzal", '\u0051'));
                category.Entries.Add(new Entry("Rand", '\u0052'));
                category.Entries.Add(new Entry("Bengali Rupee mark", '\u09F2'));
                category.Entries.Add(new Entry("Bengali Rupee sign", '\u09F3'));
                category.Entries.Add(new Entry("Shilling", '\u0053'));

                //category.Entries.Add(new Entry("Riel", '\u17DB'));
                //category.Entries.Add(new Entry("Rial", '\uFDFC'));
            }
        }

        #endregion

        #region Fractions

        private static Category fractions = null;

        public Category Fractions
        {
            get
            {
                if (fractions == null)
                {
                    fractions = new Category("Fractions");

                    Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        Loadfractions();
                    }));
                }

                return fractions;
            }
        }

        public static void Loadfractions()
        {
            if (fractions == null)
            {
                fractions = new Category("Fractions");
            }

            if (fractions.Entries.Count == 0)
            {
                Category category = fractions;

                category.Entries.Add(new Entry("1/4 fraction", '\u00BC'));
                category.Entries.Add(new Entry("1/2 fraction", '\u00BD'));
                category.Entries.Add(new Entry("3/4 fraction", '\u00BE'));
                category.Entries.Add(new Entry("1/3 fraction", '\u2153'));
                category.Entries.Add(new Entry("2/3 fraction", '\u2154'));
                category.Entries.Add(new Entry("1/5 fraction", '\u2155'));
                category.Entries.Add(new Entry("2/5 fraction", '\u2156'));
                category.Entries.Add(new Entry("3/5 fraction", '\u2157'));
                category.Entries.Add(new Entry("4/5 fraction", '\u2158'));
                category.Entries.Add(new Entry("1/6 fraction", '\u2159'));
                category.Entries.Add(new Entry("5/6 fraction", '\u215A'));
                category.Entries.Add(new Entry("1/8 fraction", '\u215B'));
                category.Entries.Add(new Entry("3/8 fraction", '\u215C'));
                category.Entries.Add(new Entry("5/8 fraction", '\u215D'));
                category.Entries.Add(new Entry("7/8 fraction", '\u215E'));
            }
        }

        #endregion

        #region Greek Lowercase

        private static Category greekLowercase = null;

        public Category GreekLowercase
        {
            get 
            {
                if (greekLowercase == null)
                {
                    greekLowercase = new Category("Greek Lowercase");

                    Dispatcher.BeginInvoke(new Action(delegate() 
                    { 
                        LoadGreekLowercase(); 
                    }));
                }

                return greekLowercase;
            }
        }

        public static void LoadGreekLowercase()
        {
            if (greekLowercase == null)
            {
                greekLowercase = new Category("Greek Lowercase");
            }

            if (greekLowercase.Entries.Count == 0)
            {
                Category category = greekLowercase;

                category.Entries.Add(new Entry("Alpha", '\u03B1'));
                category.Entries.Add(new Entry("Beta", '\u03B2'));
                category.Entries.Add(new Entry("Gamma", '\u03B3'));
                category.Entries.Add(new Entry("Delta", '\u03B4'));
                category.Entries.Add(new Entry("Epsilon", '\u03B5'));
                category.Entries.Add(new Entry("Zeta", '\u03B6'));
                category.Entries.Add(new Entry("Eta", '\u03B7'));
                category.Entries.Add(new Entry("Theta", '\u03B8'));
                category.Entries.Add(new Entry("Iota", '\u03B9'));
                category.Entries.Add(new Entry("Kappa", '\u03BA'));
                category.Entries.Add(new Entry("Lambda", '\u03BB'));
                category.Entries.Add(new Entry("Mu", '\u03BC'));
                category.Entries.Add(new Entry("Nu", '\u03BD'));
                category.Entries.Add(new Entry("Xi", '\u03BE'));
                category.Entries.Add(new Entry("Omicron", '\u03BF'));
                category.Entries.Add(new Entry("Pi", '\u03C0'));
                category.Entries.Add(new Entry("Rho", '\u03C1'));
                category.Entries.Add(new Entry("Final Sigma", '\u03C2'));
                category.Entries.Add(new Entry("Sigma", '\u03C3'));
                category.Entries.Add(new Entry("Tau", '\u03C4'));
                category.Entries.Add(new Entry("Upsilon", '\u03C5'));
                category.Entries.Add(new Entry("Phi", '\u03C6'));
                category.Entries.Add(new Entry("Chi", '\u03C7'));
                category.Entries.Add(new Entry("Psi", '\u03C8'));
                category.Entries.Add(new Entry("Omega", '\u03C9'));
            }
        }

        #endregion

        #region Greek Uppercase

        private static Category greekUppercase = null;

        public Category GreekUppercase
        {
            get
            {
                if (greekUppercase == null)
                {
                    greekUppercase = new Category("Greek Uppercase");

                    Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        LoadGreekUppercase();
                    }));
                }

                return greekUppercase;
            }
        }

        public static void LoadGreekUppercase()
        {
            if (greekUppercase == null)
            {
                greekUppercase = new Category("Greek Uppercase");
            }

            if (greekUppercase.Entries.Count == 0)
            {
                Category category = greekUppercase;

                category.Entries.Add(new Entry("Alpha", '\u0391'));
                category.Entries.Add(new Entry("Beta", '\u0392'));
                category.Entries.Add(new Entry("Gamma", '\u0393'));
                category.Entries.Add(new Entry("Delta", '\u0394'));
                category.Entries.Add(new Entry("Epsilon", '\u0395'));
                category.Entries.Add(new Entry("Zeta", '\u0396'));
                category.Entries.Add(new Entry("Eta", '\u0397'));
                category.Entries.Add(new Entry("Theta", '\u0398'));
                category.Entries.Add(new Entry("Iota", '\u0399'));
                category.Entries.Add(new Entry("Kappa", '\u039A'));
                category.Entries.Add(new Entry("Lambda", '\u039B'));
                category.Entries.Add(new Entry("Mu", '\u039C'));
                category.Entries.Add(new Entry("Nu", '\u039D'));
                category.Entries.Add(new Entry("Xi", '\u039E'));
                category.Entries.Add(new Entry("Omicron", '\u039F'));
                category.Entries.Add(new Entry("Pi", '\u03A0'));
                category.Entries.Add(new Entry("Rho", '\u03A1'));
                category.Entries.Add(new Entry("Sigma", '\u03A3'));
                category.Entries.Add(new Entry("Tau", '\u03A4'));
                category.Entries.Add(new Entry("Upsilon", '\u03A5'));
                category.Entries.Add(new Entry("Phi", '\u03A6'));
                category.Entries.Add(new Entry("Chi", '\u03A7'));
                category.Entries.Add(new Entry("Psi", '\u03A8'));
                category.Entries.Add(new Entry("Omega", '\u03A9'));
            }
        }

        #endregion

        #region Legal

        private static Category legal = null;

        public Category Legal
        {
            get
            {
                if (legal == null)
                {
                    legal = new Category("Legal");

                    Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        LoadLegal();
                    }));
                }

                return legal;
            }
        }

        public static void LoadLegal()
        {
            if (legal == null)
            {
                legal = new Category("Legal");
            }

            if (legal.Entries.Count == 0)
            {
                Category category = legal;

                category.Entries.Add(new Entry("Copyright", '\u00A9'));
                category.Entries.Add(new Entry("Registered trademark", '\u00AE'));
                category.Entries.Add(new Entry("Sound recording copyright", '\u2117'));
                category.Entries.Add(new Entry("Trademark", '\u2122'));
                category.Entries.Add(new Entry("Service mark", '\u2120'));
                category.Entries.Add(new Entry("NParagraph", '\u00B6'));
                category.Entries.Add(new Entry("Section", '\u00A7'));
                category.Entries.Add(new Entry("Plaintiff", '\u03A0'));
                category.Entries.Add(new Entry("Defendant", '\u0394'));
            }
        }

        #endregion

        #region Logic and Set Theory

        private static Category logicAndSetTheory = null;

        public Category LogicAndSetTheory
        {
            get
            {
                if (logicAndSetTheory == null)
                {
                    logicAndSetTheory = new Category("Logic & Set Theory");

                    Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        LoadLogicAndSetTheory();
                    }));
                }

                return logicAndSetTheory;
            }
        }

        public static void LoadLogicAndSetTheory()
        {
            if (logicAndSetTheory == null)
            {
                logicAndSetTheory = new Category("Logic & Set Theory");
            }

            if (logicAndSetTheory.Entries.Count == 0)
            {
                Category category = logicAndSetTheory;

                category.Entries.Add(new Entry("Tautology", '\u22A4'));
                category.Entries.Add(new Entry("Contradiction", '\u22A5'));
                category.Entries.Add(new Entry("Definition", '\u2254'));
                category.Entries.Add(new Entry("Implies", '\u2192'));
                category.Entries.Add(new Entry("If and only if", '\u2194'));
                category.Entries.Add(new Entry("Equivalent", '\u2261'));
                category.Entries.Add(new Entry("Material implication", '\u21D2'));
                category.Entries.Add(new Entry("Material equivalence", '\u21D4'));
                category.Entries.Add(new Entry("Provable", '\u22A2'));
                category.Entries.Add(new Entry("Entails", '\u22A8'));

                category.Entries.Add(new Entry("For all", '\u2200'));
                category.Entries.Add(new Entry("Complement", '\u2201'));
                category.Entries.Add(new Entry("There exists", '\u2203'));
                category.Entries.Add(new Entry("There does not exist", '\u2204'));
                category.Entries.Add(new Entry("Contains", '\u2208'));
                category.Entries.Add(new Entry("Does Not contain", '\u2209'));
                category.Entries.Add(new Entry("Contains member", '\u220B'));
                category.Entries.Add(new Entry("Does not contain member", '\u220C'));

                category.Entries.Add(new Entry("Subset of", '\u2282'));
                category.Entries.Add(new Entry("Superset of", '\u2283'));
                category.Entries.Add(new Entry("Not subset of", '\u2284'));
                category.Entries.Add(new Entry("Not superset of", '\u2285'));
                category.Entries.Add(new Entry("Subset of or equal to", '\u2286'));
                category.Entries.Add(new Entry("Superset of or equal to", '\u2287'));
                category.Entries.Add(new Entry("Neither subset nor equal", '\u2288'));
                category.Entries.Add(new Entry("Neither superset nor equal", '\u2289'));
                category.Entries.Add(new Entry("Subset of and not equal", '\u228A'));
                category.Entries.Add(new Entry("Superset of and not equal", '\u228B'));

                category.Entries.Add(new Entry("Asymptotically equal", '\u2243'));
                category.Entries.Add(new Entry("Not asymptotically equal", '\u2244'));

                category.Entries.Add(new Entry("Not", '\u00AC'));
                category.Entries.Add(new Entry("And", '\u2227'));
                category.Entries.Add(new Entry("Or", '\u2228'));
                category.Entries.Add(new Entry("XOR", '\u22BB'));
                category.Entries.Add(new Entry("NAND", '\u22BC'));
                category.Entries.Add(new Entry("NOR", '\u22BD'));
                category.Entries.Add(new Entry("Union", '\u222A'));
                category.Entries.Add(new Entry("Intersection", '\u2229'));

                category.Entries.Add(new Entry("Real numbers", '\u211D'));
                category.Entries.Add(new Entry("Complex numbers", '\u2102'));
                category.Entries.Add(new Entry("Natural numbers", '\u2115'));
                category.Entries.Add(new Entry("Prime numbers", '\u2119'));
                category.Entries.Add(new Entry("Rational numbers", '\u211A'));
                category.Entries.Add(new Entry("Integers", '\u2124'));
                category.Entries.Add(new Entry("Empty set", '\u2205'));
            }
        }

        #endregion

        #region Math

        private static Category math = null;

        public Category Math
        {
            get
            {
                if (math == null)
                {
                    math = new Category("Math");

                    Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        LoadMath();
                    }));
                }

                return math;
            }
        }

        public static void LoadMath()
        {
            if (math == null)
            {
                math = new Category("Math");
            }

            if (math.Entries.Count == 0)
            {
                Category category = math;

                category.Entries.Add(new Entry("Equals", '='));
                category.Entries.Add(new Entry("Almost equals", '\u2248'));
                category.Entries.Add(new Entry("Not equals", '\u2260'));
                category.Entries.Add(new Entry("Identical to", '\u2261'));
                category.Entries.Add(new Entry("Not identical to", '\u2262'));
                category.Entries.Add(new Entry("Congruent to", '\u2245'));
                category.Entries.Add(new Entry("Less than", '\u003C'));
                category.Entries.Add(new Entry("Greater than", '\u003E'));
                category.Entries.Add(new Entry("Less than or equal to", '\u2264'));
                category.Entries.Add(new Entry("Greater than or equal to", '\u2265'));
                category.Entries.Add(new Entry("Proportional to", '\u221D'));
                category.Entries.Add(new Entry("Plus", '+'));
                category.Entries.Add(new Entry("Minus", '\u2212'));
                category.Entries.Add(new Entry("Plus or minus", '\u00B1'));
                category.Entries.Add(new Entry("Times", '\u00D7'));
                category.Entries.Add(new Entry("Divided by", '\u00F7'));
                category.Entries.Add(new Entry("Dot operator", '\u22C5'));
                category.Entries.Add(new Entry("Fraction", '\u2044'));
                category.Entries.Add(new Entry("Square root", '\u221A'));
                //category.Entries.Add(new Entry("Cube root", '\u221B'));
                //category.Entries.Add(new Entry("Fourth root", '\u221C'));
                //category.Entries.Add(new Entry("Squared", '\u00B2'));
                //category.Entries.Add(new Entry("Cubed", '\u00B3'));
                //category.Entries.Add(new Entry("Fourth power", '\u2074'));
                category.Entries.Add(new Entry("Integral", '\u222B'));
                category.Entries.Add(new Entry("Closed integral", '\u222E'));
                category.Entries.Add(new Entry("Partial differential", '\u2202'));
                category.Entries.Add(new Entry("Increment", '\u2206'));
                category.Entries.Add(new Entry("Gradient", '\u2207'));
                category.Entries.Add(new Entry("Sum", '\u2211'));
                category.Entries.Add(new Entry("N-ary product", '\u220F'));
                category.Entries.Add(new Entry("N-ary coproduct", '\u2210'));
                category.Entries.Add(new Entry("Vertical bar", '\u007C'));
                category.Entries.Add(new Entry("Divides", '\u2223'));
                category.Entries.Add(new Entry("Norm", '\u2016'));
                category.Entries.Add(new Entry("Left ceiling", '\u2308'));
                category.Entries.Add(new Entry("Right ceiling", '\u2309'));
                category.Entries.Add(new Entry("Left floor", '\u230A'));
                category.Entries.Add(new Entry("Right floor", '\u230B'));
                category.Entries.Add(new Entry("Prime", '\u2032'));
                category.Entries.Add(new Entry("Double prime", '\u2033'));
                category.Entries.Add(new Entry("Triple prime", '\u2034'));
                category.Entries.Add(new Entry("Infinity", '\u221E'));
                category.Entries.Add(new Entry("Therefore", '\u2234'));
                category.Entries.Add(new Entry("Because", '\u2235'));
                category.Entries.Add(new Entry("End of proof", '\u220E'));
            }
        }

        #endregion

        #region Punctuation

        private static Category punctuation = null;

        public Category Punctuation
        {
            get
            {
                if (punctuation == null)
                {
                    punctuation = new Category("Punctuation");

                    Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        LoadPunctuation();
                    }));
                }

                return punctuation;
            }
        }

        public static void LoadPunctuation()
        {
            if (punctuation == null)
            {
                punctuation = new Category("Punctuation");
            }

            if (punctuation.Entries.Count == 0)
            {
                Category category = punctuation;

                category.Entries.Add(new Entry("Apostrophe", '\''));
                category.Entries.Add(new Entry("Left parenthesis", '('));
                category.Entries.Add(new Entry("Right parenthesis", ')'));
                category.Entries.Add(new Entry("Left square bracket", '['));
                category.Entries.Add(new Entry("Right square bracket", ']'));
                category.Entries.Add(new Entry("Left curly bracket", '{'));
                category.Entries.Add(new Entry("Right curly bracket", '}'));
                category.Entries.Add(new Entry("Left angle bracket", '\u3008'));
                category.Entries.Add(new Entry("Right angle bracket", '\u3009'));
                category.Entries.Add(new Entry("Colon", ':'));
                category.Entries.Add(new Entry("Comma", ','));
                category.Entries.Add(new Entry("En dash", '\u2013'));
                category.Entries.Add(new Entry("Em dash", '\u2014'));
                category.Entries.Add(new Entry("Ellipsis", '\u2026'));
                category.Entries.Add(new Entry("Exclamation mark", '!'));
                category.Entries.Add(new Entry("Period", '.'));
                category.Entries.Add(new Entry("Left guillemet", '\u00AB'));
                category.Entries.Add(new Entry("Right guillemet", '\u00BB'));
                category.Entries.Add(new Entry("Hyphen", '\u2010'));
                category.Entries.Add(new Entry("Hyphen-minus", '\u002D'));
                category.Entries.Add(new Entry("Question mark", '?'));
                category.Entries.Add(new Entry("Neutral single quote", '\u0027'));
                category.Entries.Add(new Entry("Neutral double quote", '\u0022'));
                category.Entries.Add(new Entry("Left single quote", '\u2018'));
                category.Entries.Add(new Entry("Right single quote", '\u2019'));
                category.Entries.Add(new Entry("Left double quote", '\u201C'));
                category.Entries.Add(new Entry("Right double quote", '\u201D'));
                category.Entries.Add(new Entry("Semicolon", ';'));
                category.Entries.Add(new Entry("Forward slash", '/'));
                category.Entries.Add(new Entry("Solidus", '\u2044'));
            }
        }


        #endregion

        #region Typography

        private static Category typography = null;

        public Category Typography
        {
            get
            {
                if (typography == null)
                {
                    typography = new Category("Typography");

                    Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        LoadTypography();
                    }));
                }

                return typography;
            }
        }

        public static void LoadTypography()
        {
            if (typography == null)
            {
                typography = new Category("Typography");
            }

            if (typography.Entries.Count == 0)
            {
                Category category = typography;

                category.Entries.Add(new Entry("Ampersand", '&'));
                category.Entries.Add(new Entry("At", '@'));
                category.Entries.Add(new Entry("Asterisk", '*'));
                category.Entries.Add(new Entry("Backslash", '\\'));
                category.Entries.Add(new Entry("Bullet", '\u2022'));
                category.Entries.Add(new Entry("Caret", '^'));
                category.Entries.Add(new Entry("Dagger/obelisk", '\u2020'));
                category.Entries.Add(new Entry("Double-dagger/diesis", '\u2021'));
                category.Entries.Add(new Entry("Degree", '\u00B0'));
                category.Entries.Add(new Entry("Ditto", '\u3003'));
                category.Entries.Add(new Entry("Inverted exclamation mark", '\u00A1'));
                category.Entries.Add(new Entry("Inverted question mark", '\u00BF'));
                category.Entries.Add(new Entry("Hash", '#'));
                category.Entries.Add(new Entry("Numero sign", '\u2116'));
                category.Entries.Add(new Entry("Obelus", '\u00F7'));
                category.Entries.Add(new Entry("Ordinal indicator", '\u00BA'));
                category.Entries.Add(new Entry("Ordinal indicator", '\u00AA'));
                category.Entries.Add(new Entry("Percent", '\u0025'));
                category.Entries.Add(new Entry("Permille", '\u2030'));
                category.Entries.Add(new Entry("Permyriad", '\u2031'));
                category.Entries.Add(new Entry("Pilcrow", '\u00B6'));
                category.Entries.Add(new Entry("Prime", '\u2032'));
                category.Entries.Add(new Entry("Double prime", '\u2033'));
                category.Entries.Add(new Entry("Triple prime", '\u2034'));
                category.Entries.Add(new Entry("Section", '\u00A7'));
                category.Entries.Add(new Entry("Tilde", '~'));
                category.Entries.Add(new Entry("Underscore", '_'));
                category.Entries.Add(new Entry("Vertical", '\u007C'));
                category.Entries.Add(new Entry("Broken bar", '\u00A6'));

            }
        }

        #endregion

    }
}
