/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2009-01-09: Added IsValidJmbg method.
//2009-01-05: Added support for OIB.
//            Obsoleted constructor with JMBG only in order to ease transition.
//2008-11-05: Decreased cyclomatic complexity.
//2008-05-31: Added IsBirthDateValid
//2008-04-11: Cleaned code to match FxCop 1.36 beta 2 (NestedTypesShouldNotBeVisible).
//            Added Republic of Kosovo.
//2008-02-15: New version.


using System;

namespace Medo.Localization.Croatia {

    /// <summary>
    /// Handling JMBG/OIB data.
    /// </summary>
    public class Jmbg {

        /// <summary>
        /// Creates new instance based on given JMBG.
        /// </summary>
        /// <param name="value">JMBG.</param>
        /// <remarks>All JMBG's with year digits lower than 800 will be considered as year 2xxx.</remarks>
        [Obsolete("Please use overload that specifies wheter OIB is to be parsed also.")]
        public Jmbg(string value) : this(value, false) { }

        /// <summary>
        /// Creates new instance based on given JMBG/OIB.
        /// </summary>
        /// <param name="value">JMBG/OIB.</param>
        /// <param name="parseOib">If true, OIB is also parsed.</param>
        /// <remarks>All JMBG's with year digits lower than 800 will be considered as year 2xxx.</remarks>
        public Jmbg(string value, bool parseOib) {
            Value = value;

            if (parseOib && (value.Length == 11)) { //this is OIB
                var sum = 10;
                for (var i = 0; i < 10; ++i) {
                    if ((value[i] >= '0') && (value[i] <= '9')) {
                        sum += (value[i] - '0');
                        if (sum > 10) { sum -= 10; }
                        sum *= 2;
                        if (sum >= 11) { sum -= 11; }
                    } else {
                        IsValid = false;
                        return;
                    }
                }
                char checkDigit;
                var sum2 = 11 - sum;
                if (sum2 == 10) {
                    checkDigit = '0';
                } else {
                    checkDigit = System.Convert.ToChar('0' + sum2);
                }
                if (value[10] == checkDigit) {
                    IsValid = true;
                    IsBirthDateValid = false;
                    Region = JmbgRegion.Unknown;
                    Gender = JmbgGender.Unknown;
                } else {
                    IsValid = false;
                }
                return;
            }

            try {
                if (value.Length >= 7) { //extract date
                    var birthDay = int.Parse(value.Substring(0, 2), System.Globalization.CultureInfo.InvariantCulture);
                    var birthMonth = int.Parse(value.Substring(2, 2), System.Globalization.CultureInfo.InvariantCulture);
                    var birthYear = int.Parse(value.Substring(4, 3), System.Globalization.CultureInfo.InvariantCulture) + 1000;
                    if (birthYear < 1800) { birthYear += 1000; }

                    try {
                        var birthDate = new DateTime(birthYear, birthMonth, birthDay);
                        if ((birthDate.ToString("ddMM", System.Globalization.CultureInfo.InvariantCulture) + birthDate.ToString("yyyy", System.Globalization.CultureInfo.InvariantCulture).Remove(0, 1)) != value.Substring(0, 7)) { //date is invalid
                            return;
                        }
                        BirthDate = birthDate;
                        IsBirthDateValid = birthDate <= DateTime.Today;
                    } catch (System.ArgumentOutOfRangeException) { //date is invalid
                        return;
                    }
                }

                if (value.Length != 13) { //invalid length
                    return;
                }


                var digits = new int[13];

                for (var i = 0; i < value.Length; i++) {
                    if (char.IsDigit(value[i])) {
                        digits[i] = int.Parse(value.Substring(i, 1), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                    } else { //invalid characters
                        return;
                    }
                }


                var mask = new int[] { 7, 6, 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
                var sum = 0;
                for (var i = 0; i < 12; i++) {
                    sum += digits[i] * mask[i];
                }

                int checksum;
                switch (sum % 11) {
                    case 0:
                        checksum = 0;
                        break;
                    case 1: //invalid number
                        return;
                    default:
                        checksum = 11 - sum % 11;
                        break;
                }
                if (checksum != digits[12]) { //checksum mismatch
                    return;
                }


                var regionDigits = value.Substring(7, 2);
                if (regionDigits == "03") {
                    Region = JmbgRegion.Foreign;
                } else if (regionDigits.StartsWith("1", StringComparison.Ordinal)) {
                    Region = JmbgRegion.BosniaAndHerzegovina;
                } else if (regionDigits.StartsWith("2", StringComparison.Ordinal)) {
                    Region = JmbgRegion.Montenegro;
                } else if (regionDigits.StartsWith("3", StringComparison.Ordinal)) {
                    Region = JmbgRegion.Croatia;
                } else if (regionDigits.StartsWith("4", StringComparison.Ordinal)) {
                    Region = JmbgRegion.Macedonia;
                } else if (regionDigits.StartsWith("5", StringComparison.Ordinal)) {
                    Region = JmbgRegion.Slovenia;
                } else if (regionDigits.StartsWith("7", StringComparison.Ordinal)) {
                    Region = JmbgRegion.Serbia;
                } else if (regionDigits.StartsWith("8", StringComparison.Ordinal)) {
                    Region = JmbgRegion.SerbiaVojvodina;
                } else if (regionDigits.StartsWith("9", StringComparison.Ordinal)) {
                    Region = JmbgRegion.RepublicOfKosovo;
                } else {
                    return;
                }


                if (int.Parse(value.Substring(9, 3), System.Globalization.CultureInfo.InvariantCulture) < 500) {
                    Gender = JmbgGender.Male;
                } else {
                    Gender = JmbgGender.Female;
                }
            } catch (FormatException) {
                return;
            }

            IsValid = true;
        }


        /// <summary>
        /// Returns JMBG/OIB.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Returns birth date.
        /// </summary>
        public DateTime BirthDate { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Returns region.
        /// </summary>
        public JmbgRegion Region { get; private set; } = JmbgRegion.Unknown;

        /// <summary>
        /// Returns gender.
        /// </summary>
        public JmbgGender Gender { get; private set; } = JmbgGender.Unknown;

        /// <summary>
        /// Returns true if JMBG/OIB is valid.
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Returns true if birth date part is valid.
        /// </summary>
        public bool IsBirthDateValid { get; private set; }

        /// <summary>
        /// Returns true if given valid JMBG.
        /// </summary>
        /// <param name="jmbg">JMBG to check.</param>
        /// <param name="parseOib">If true, OIB is also allowed.</param>
        public static bool IsValidJmbg(string jmbg, bool parseOib) {
            var instance = new Jmbg(jmbg, parseOib);
            return instance.IsValid;
        }

        /// <summary>
        /// Returns JMBG/OIB if one is valid.
        /// </summary>
        public override string ToString() {
            if (IsValid) {
                return Value;
            } else {
                if (IsBirthDateValid) {
                    return BirthDate.ToString("ddMM", System.Globalization.CultureInfo.InvariantCulture) + (BirthDate.Year % 1000).ToString("000", System.Globalization.CultureInfo.InvariantCulture);
                } else {
                    return string.Empty;
                }
            }
        }

    }



    /// <summary>
    /// Gender information.
    /// </summary>
    public enum JmbgGender {
        /// <summary>
        /// Unknown gender.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Male gender.
        /// </summary>
        [System.ComponentModel.Description("Male")]
        Male = 1,
        /// <summary>
        /// Female gender.
        /// </summary>
        [System.ComponentModel.Description("Female")]
        Female = 2,
    }



    /// <summary>
    /// Region information.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags", Justification = "These are not flags.")]
    public enum JmbgRegion {
        /// <summary>
        /// Unknown region.
        /// </summary>
        [System.ComponentModel.Description("Unknown")]
        Unknown = 0,
        /// <summary>
        /// Bosnia and Herzegovina.
        /// </summary>
        [System.ComponentModel.Description("Bosnia and Herzegovina")]
        BosniaAndHerzegovina = 1,
        /// <summary>
        /// Montenegro.
        /// </summary>
        [System.ComponentModel.Description("Montenegro")]
        Montenegro = 2,
        /// <summary>
        /// Croatia.
        /// </summary>
        [System.ComponentModel.Description("Croatia")]
        Croatia = 3,
        /// <summary>
        /// Former Yugoslav Republic of Macedonia.
        /// </summary>
        [System.ComponentModel.Description("Macedonia")]
        Macedonia = 4,
        /// <summary>
        /// Slovenia.
        /// </summary>
        [System.ComponentModel.Description("Slovenia")]
        Slovenia = 5,
        /// <summary>
        /// Serbia.
        /// </summary>
        [System.ComponentModel.Description("Serbia")]
        Serbia = 7,
        /// <summary>
        /// Vojvodina (Serbia).
        /// </summary>
        [System.ComponentModel.Description("Vojvodina (Serbia)")]
        SerbiaVojvodina = 8,
        /// <summary>
        /// Kosovo (Serbia).
        /// </summary>
        [System.ComponentModel.Description("Kosovo (Serbia)")]
        [Obsolete("In February 2008, Kosovo declared the territory's independence as the Republic of Kosovo. Please use RepublicOfKosovo instead.")]
        SerbiaKosovo = 9,
        /// <summary>
        /// Republic of Kosovo.
        /// </summary>
        [System.ComponentModel.Description("Republic of Kosovo")]
        RepublicOfKosovo = 9,
        /// <summary>
        /// Foreign.
        /// </summary>
        [System.ComponentModel.Description("Foreign")]
        Foreign = 10
    }

}
