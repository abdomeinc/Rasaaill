using System;
using System.Collections.Generic;
using System.IO; // For Stream
using System.Linq; // For .Any()
using System.Reflection; // For Assembly
using System.Windows.Media;
using System.Threading.Tasks; // For Task.Run
using System.Windows.Resources; // For Application.GetResourceStream
using SharpVectors.Renderers.Wpf;
using System.Windows;
using SharpVectors.Converters;
using System.Diagnostics; // For FileSvgReader and WpfDrawingSettings

namespace Client.WPF.Helpers
{
    public static class EmojiManager
    {
        public static List<Models.Emoji> _emojisList = new List<Models.Emoji>();
        private static Dictionary<string, DrawingGroup> _emojiCache = new Dictionary<string, DrawingGroup>();
        private static WpfDrawingSettings _wpfDrawingSettings;

        // Base path within your assembly resources
        private const string BaseResourcePath = "Resources/Emojis/";

        static EmojiManager()
        {
            _wpfDrawingSettings = new WpfDrawingSettings();
            // Customize settings if needed, e.g., for consistent text rendering
            _wpfDrawingSettings.TextAsGeometry = true;
        }

        /// <summary>
        /// Asynchronously retrieves a cached or loads a new DrawingGroup for a given emoji code from embedded resources.
        /// </summary>
        /// <param name="emojiCode">The code representing the emoji (e.g., "u2197" from emoji_u2197.svg).</param>
        /// <returns>A DrawingGroup representing the emoji, or null if not found/loaded.</returns>
        public static async Task<DrawingGroup?> GetEmojiDrawingAsync(string emojiCode)
        {
            //// Normalize emojiCode to match the resource file naming convention if necessary
            //// For example, if your files are emoji_u2197.svg, and emojiCode is "2197", prepend "u".
            //// Adjust this based on your exact file naming and emojiCode format.
            string formattedEmojiCode = $"emoji_{emojiCode}.svg"; // Assuming 'u' prefix for unicode is in the file name
            //// If your emojiCode already includes the 'u' like "u2197", then use:
            //// string formattedEmojiCode = $"emoji_{emojiCode}.svg";

            //if (_emojiCache.TryGetValue(formattedEmojiCode, out DrawingGroup? cachedDrawing))
            //{
            //    return cachedDrawing;
            //}

            //// Construct the pack URI for the embedded resource
            //Uri resourceUri = new Uri($"pack://application:,,,/{BaseResourcePath}{formattedEmojiCode}", UriKind.Absolute);

            //DrawingGroup? loadedDrawingGroup = null;

            //try
            //{
            //    // Task.Run to offload stream opening and SVG parsing to a background thread
            //    loadedDrawingGroup = await Task.Run(() =>
            //    {
            //        Stream? svgStream = null;
            //        try
            //        {
            //            // Open the resource stream. This must be done on the UI thread or a thread that
            //            // has access to Application.Current.
            //            // However, Task.Run creates a new thread, so Application.GetResourceStream might
            //            // sometimes have issues if called directly within Task.Run on a fresh thread
            //            // that doesn't have the WPF Dispatcher context.
            //            // A more robust approach is to pass the stream or Uri to the background task.
            //            // For simplicity, we'll try to get it here. If it fails, we'll refine.

            //            // The proper way to get resource stream in a background thread context
            //            // is often to make sure the Uri is fully resolved first or pass the stream.
            //            // For a pack URI, it's generally safe.
            //            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

            //            if (streamInfo?.Stream != null)
            //            {
            //                svgStream = streamInfo.Stream;

            //                // SharpVectors' FileSvgReader can also read from a Stream
            //                var reader = new FileSvgReader(_wpfDrawingSettings);
            //                DrawingGroup? drawing = reader.Read(svgStream); // Read from stream

            //                if (drawing != null)
            //                {
            //                    // Freeze on the background thread if possible for better performance
            //                    if (drawing.CanFreeze)
            //                    {
            //                        drawing.Freeze();
            //                    }
            //                    return drawing;
            //                }
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Debug.WriteLine(ex.Message);
            //        }
            //        finally
            //        {
            //            svgStream?.Dispose(); // Ensure the stream is closed
            //        }
            //        return null;
            //    });

            //    if (loadedDrawingGroup != null)
            //    {
            //        _emojiCache[formattedEmojiCode] = loadedDrawingGroup;
            //        return loadedDrawingGroup;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    // Log the exception, e.g., using a logging framework
            //    Console.WriteLine($"Error loading embedded SVG for emoji {emojiCode} ({formattedEmojiCode}): {ex.Message}");
            //    // In debug, you might want to break here to see the error more clearly:
            //    // System.Diagnostics.Debug.WriteLine($"Error: {ex}");
            //}


            string resourceName = $"{Assembly.GetExecutingAssembly().GetName().Name}.{BaseResourcePath.Replace('/', '.')}{formattedEmojiCode}";

            if (_emojiCache.TryGetValue(formattedEmojiCode, out DrawingGroup? cachedDrawing))
            {
                return cachedDrawing;
            }

            DrawingGroup? loadedDrawingGroup = null;

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using Stream? svgStream = assembly.GetManifestResourceStream(resourceName);

                if (svgStream != null)
                {
                    loadedDrawingGroup = await Task.Run(() =>
                    {
                        try
                        {
                            var reader = new FileSvgReader(_wpfDrawingSettings);
                            DrawingGroup? drawing = reader.Read(svgStream);
                            if (drawing?.CanFreeze == true)
                            {
                                drawing.Freeze();
                            }
                            return drawing;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error parsing SVG from embedded resource '{resourceName}': {ex.Message}");
                            return null;
                        }
                    });

                    if (loadedDrawingGroup != null)
                    {
                        _emojiCache[formattedEmojiCode] = loadedDrawingGroup;
                    }
                    else
                    {
                        Debug.WriteLine($"Warning: Failed to load DrawingGroup from embedded resource '{resourceName}'.");
                    }
                }
                else
                {
                    Debug.WriteLine($"Error: Could not find embedded resource '{resourceName}'.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading embedded SVG for emoji {emojiCode} ({formattedEmojiCode}): {ex.Message}");
            }

            return loadedDrawingGroup;

            //return null; // Return null if loading fails
        }

        // --- Helper for loading all emoji metadata (for the picker) ---

        /// <summary>
        /// Loads metadata for all emojis by discovering embedded SVG resources.
        /// This method indexes your embedded SVG files to create a list of Emoji objects.
        /// </summary>
        /// <returns>A list of Emoji objects with Code, but Drawing is initially null.</returns>
        public static async Task LoadAllEmojiMetadataAsync()
        {
            _emojisList = new List<Models.Emoji>();

            await Task.Run(() =>
            {
                Assembly currentAssembly = Assembly.GetExecutingAssembly();
                string[] resourceNames = currentAssembly.GetManifestResourceNames();

                // Resource names are typically like "YourAssembly.Resources.Emojis.emoji_u2197.svg"
                var svgResourceNames = resourceNames
                    .Where(name => name.StartsWith($"{currentAssembly.GetName().Name}.{BaseResourcePath.Replace('/', '.')}", StringComparison.OrdinalIgnoreCase) &&
                                   name.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (string fullResourceName in svgResourceNames)
                {
                    // Extract the emoji code (e.g., "u2197" from "YourAssembly.Resources.Emojis.emoji_u2197.svg")
                    string fileName = fullResourceName.Substring(fullResourceName.LastIndexOf('.') + 1); // e.g., "svg"
                    fileName = fullResourceName.Substring(0, fullResourceName.Length - (fileName.Length + 1)); // remove .svg

                    // This gets "YourAssembly.Resources.Emojis.emoji_u2197"
                    string emojiCodeWithPrefix = fileName.Substring(fileName.LastIndexOf('.') + 1); // e.g., "emoji_u2197"
                    string emojiCode = emojiCodeWithPrefix.Replace("emoji_", ""); // e.g., "u2197"

                    // For DisplayCharacter and Description, you'll still need a mapping.
                    // If you don't have a separate JSON/CSV, you'll need to generate a placeholder,
                    // or implement a way to get them from a standard Unicode emoji data file.
                    // For now, we'll use placeholders.
                    string displayChar = GetDisplayCharacterFromCode(emojiCode); // Implement this
                    string description = GetDescriptionFromCode(emojiCode);     // Implement this

                    _emojisList.Add(new Models.Emoji
                    {
                        Code = emojiCode,
                        DisplayCharacter = displayChar,
                        Description = description
                    });
                }
            });

            //return _emojisList;
        }

        // --- Placeholder for mapping emoji codes to characters/descriptions ---
        // This is a simplified example. For full Noto Emojis, you'd integrate with
        // an emoji data file (like emoji-test.txt or a JSON derived from it).
        private static string GetDisplayCharacterFromCode(string code)
        {
            // Example: "u2197" -> "\u2197"
            // This is complex for multi-codepoint emojis (e.g., skin tones, flags).
            // A lookup table is much better.
            try
            {
                // This handles basic Unicode characters but not complex emojis like flags or skin tones
                // which are multiple codepoints.
                if (code.StartsWith("u", StringComparison.OrdinalIgnoreCase))
                {
                    code = code.Substring(1); // Remove 'u'
                }

                // If it's a sequence like "1F468_200D_1F469_200D_1F467_200D_1F466"
                // You'd need to split by underscores and convert each part
                if (code.Contains("_"))
                {
                    // This is a complex mapping that usually requires a proper emoji data library
                    // or a pre-generated lookup table.
                    return "❓"; // Fallback for complex sequences
                }

                int unicodeValue = Convert.ToInt32(code, 16);
                return char.ConvertFromUtf32(unicodeValue);
            }
            catch
            {
                return "❓"; // Fallback character
            }
        }

        private static string GetDescriptionFromCode(string code)
        {
            // This would also come from a robust emoji data source/lookup.
            // For now, a simple placeholder.
            return $"Emoji {code}";
        }
    }
}