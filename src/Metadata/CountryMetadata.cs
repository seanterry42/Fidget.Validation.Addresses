﻿using Fidget.Validation.Addresses.Metadata.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Fidget.Validation.Addresses.Metadata
{
    /// <summary>
    /// Country-level metadata.
    /// </summary>

    public class CountryMetadata : RegionalMetadata
    {
        /// <summary>
        /// Gets the collection of languages for which regional data is available (if known).
        /// </summary>
        /// <remarks>
        /// <see cref="!:https://github.com/googlei18n/libaddressinput/blob/master/common/src/main/java/com/google/i18n/addressinput/common/AddressDataKey.java"/>
        /// The languages used by any data for this region, if known.
        /// <para>In practice, this is only defined at the country level.</para>
        /// <para>The first entry in this list is the default language (uses no language argument in the identifier).</para>
        /// </remarks>

        [JsonProperty("languages")]
        [JsonConverter(typeof(TildeDelimitedStringConverter))]
        public IEnumerable<string> Languages { get; internal set; }

        /// <summary>
        /// Gets a string that represents the address format in the country.
        /// The sequence %n indicates a line feed. 
        /// All other characters preceded by a percent (%) character are address elements that are allowed in the address (in order).
        /// Characters that are not preceded by a percent (%) character are assumed to be literals.
        /// </summary>
        /// <remarks>
        /// <see cref="!:https://github.com/googlei18n/libaddressinput/blob/master/common/src/main/java/com/google/i18n/addressinput/common/AddressDataKey.java"/>
        /// The standard format string. 
        /// This identifies which fields can be used in the address, along with their order. 
        /// This also carries additional information for use in formatting the fields into multiple lines. 
        /// This is also used to indicate which fields should _not_ be used for an address.
        /// <para>In practice, this is only defined at the country level.</para>
        /// </remarks>

        [JsonProperty("fmt")]
        public string Format { get; internal set; }

        /// <summary>
        /// Get an alternate value of <see cref="Format"/> to use when formatting an address in a latin script.
        /// </summary>
        /// <remarks>
        /// <see cref="!:https://github.com/googlei18n/libaddressinput/blob/master/common/src/main/java/com/google/i18n/addressinput/common/AddressDataKey.java"/>
        /// The latin format string <see cref="Format"/> used when a country defines an alternative format for use with 
        /// the latin script, such as in China.
        /// </remarks>

        [JsonProperty("lfmt")]
        public string LatinFormat { get; internal set; }

        /// <summary>
        /// Gets the address elements that should be uppercased for valid addresses in the country.
        /// </summary>
        
        [JsonProperty("upper")]
        [JsonConverter(typeof(AddressFieldCollectionConverter))]
        public IEnumerable<AddressField> Uppercase { get; internal set; }

        /// <summary>
        /// Gets the name used to describe the state/province level region in the country.
        /// </summary>
        /// <remarks>
        /// <see cref="!:https://github.com/googlei18n/libaddressinput/blob/master/common/src/main/java/com/google/i18n/addressinput/common/AddressDataKey.java"/>
        /// Indicates the type of the name used for the state (administrative area) field.
        /// </remarks>

        [JsonProperty("state_name_type")]
        public string StateType { get; internal set; }

        /// <summary>
        /// Gets the name used to describe the city/locality level region in the country.
        /// </summary>
        /// <remarks>
        /// <see cref="!:https://github.com/googlei18n/libaddressinput/blob/master/common/src/main/java/com/google/i18n/addressinput/common/AddressDataKey.java"/>
        /// Indicates the type of the name used for the locality (city) field.
        /// </remarks>

        [JsonProperty("locality_name_type")]
        public string LocalityType { get; internal set; }

        /// <summary>
        /// Gets the name used to describe the sub-locality level region in the country.
        /// </summary>
        /// <remarks>
        /// <see cref="!:https://github.com/googlei18n/libaddressinput/blob/master/common/src/main/java/com/google/i18n/addressinput/common/AddressDataKey.java"/>
        /// Indicates the type of the name used for the sublocality field.
        /// </remarks>

        [JsonProperty("sublocality_name_type")]
        public string SublocalityType { get; internal set; }

        /// <summary>
        /// Gets the name used to describe the postal code in the country.
        /// </summary>
        /// <remarks>
        /// <see cref="!:https://github.com/googlei18n/libaddressinput/blob/master/common/src/main/java/com/google/i18n/addressinput/common/AddressDataKey.java"/>
        /// Indicates the type of the name used for the <see cref="RegionalMetadata.PostalCodePattern"/> field.
        /// </remarks>

        [JsonProperty("zip_name_type")]
        public string PostalCodeType { get; internal set; }
    }
}