using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuneFlow.Core.Models
{
    internal class Playlist
    {
        public string Name { get; set; }
        public List<Track> Tracks { get; set; } = new List<Track>();
    }
}
