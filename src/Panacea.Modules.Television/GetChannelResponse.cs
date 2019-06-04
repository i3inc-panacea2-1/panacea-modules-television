using Panacea.Models;
using Panacea.Modularity.Media;
using Panacea.Modularity.Media.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Television
{
    [DataContract]
    public class GetChannelsResponse
    {
        [DataMember(Name = "Television")]
        public Television Television { get; set; }
    }

    [DataContract]
    public class Television
    {
        [DataMember(Name = "channels")]
        public List<ServerChannel> Channels { get; set; }

    }

    [DataContract]
    public class ServerChannel : ServerItem
    {
        [DataMember(Name = "analog")]
        public Analog Analog { get; set; }

        [DataMember(Name = "digital")]
        public Digital Digital { get; set; }

        [DataMember(Name = "chosenDefault")]
        public bool IsDefault { get; set; }

        [DataMember(Name = "iptv")]
        public Iptv Iptv { get; set; }

        [DataMember(Name = "rogersWeb")]
        public RogersWeb RogersWebChannel { get; set; }

        public MediaItem GetChannel()
        {
            if (Analog != null)
            {

                return new AnalogMedia()
                {
                    ChannelNumber = Int32.Parse(Analog.Channel ?? Analog.Frequency),
                    CountryCode = Analog.CountryCode,
                    Country = Analog.Country,
                    Name = Name,
                    Id = Id,
                    Source = Analog.InputType
                };

            }

            if (Digital != null)
                switch (Digital.Type)
                {
                    case "dvb-t":
                        return new DvbtMedia()
                        {
                            Frequency = Int32.Parse(Digital.Frequency),
                            Name = Name,
                            Id = Id,
                            Bandwidth = Int32.Parse(Digital.Bandwidth),
                            Program = Int32.Parse(Digital.Program),
                        };
                    case "atsc":
                        return new AtscMedia()
                        {
                            Physical = Int32.Parse(Digital.PhysicalChannel),
                            Major = Int32.Parse(Digital.MajorChannel),
                            Minor = Int32.Parse(Digital.MinorChannel),
                            Name = Name,
                            Id = Id
                        };
                }

            if (Iptv != null)
                return new IptvMedia()
                {
                    Name = Name,
                    Id = Id,
                    URL = Iptv.Url
                };

            if (RogersWebChannel != null)
                return new RogersWebMedia()
                {
                    Name = Name,
                    Id = Id,
                    URL = RogersWebChannel.Url
                };

            return null;
        }
    }

    [DataContract]
    public class Analog
    {
        [DataMember(Name = "country_code")]
        public string CountryCode { get; set; }

        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "input_type")]
        public string InputType { get; set; }

        [DataMember(Name = "channel")]
        public string Channel { get; set; }

        [DataMember(Name = "frequency")]
        public string Frequency { get; set; }
    }

    [DataContract]
    public class Digital
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "majorChannel")]
        public string MajorChannel { get; set; }

        [DataMember(Name = "physicalChannel")]
        public string PhysicalChannel { get; set; }

        [DataMember(Name = "minorChannel")]
        public string MinorChannel { get; set; }

        [DataMember(Name = "frequency")]
        public string Frequency { get; set; }

        [DataMember(Name = "bandwidth")]
        public string Bandwidth { get; set; }

        [DataMember(Name = "program")]
        public string Program { get; set; }
    }

    [DataContract]
    public class Iptv
    {
        [DataMember(Name = "url")]
        public string Url { get; set; }
    }

    [DataContract]
    public class RogersWeb
    {
        [DataMember(Name = "url")]
        public string Url { get; set; }
    }
}
