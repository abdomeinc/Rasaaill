using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Client.WPF.Models
{
    public class Emoji
    {
        public string Code { get; set; } = string.Empty; // e.g., "1F600" (the file name without .svg)
        public string DisplayCharacter { get; set; } = string.Empty; // e.g., "😀" (for TextBlock or tooltip)
        public string Description { get; set; } = string.Empty; // e.g., "Grinning Face" (for search/tooltip)
                                                                // Add other properties if needed, like categories, keywords, etc.

        // A property to hold the loaded DrawingGroup, but it's loaded on demand
        // This will be null until the emoji is requested to be displayed
        public DrawingGroup? Drawing { get; set; }

        // Consider adding a path to the SVG file if you want to avoid assuming a convention
        // public string SvgFilePath { get; set; }

    }
}
