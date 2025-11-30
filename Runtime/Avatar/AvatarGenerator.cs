using System;
using UnityEngine;

namespace Aarware.Avatar {
    /// <summary>
    /// Generates unique avatar images based on a seed or string input.
    /// Uses deterministic algorithm to create consistent avatars for the same input.
    /// </summary>
    public static class AvatarGenerator {
        /// <summary>
        /// Generates an avatar sprite from a string (username, email, etc.).
        /// </summary>
        public static Sprite GenerateFromString(string input, int size = 256) {
            if (string.IsNullOrEmpty(input)) {
                Debug.LogWarning("[AvatarGenerator] Input string is null or empty. Using default.");
                input = "default";
            }

            int seed = GetSeedFromString(input);
            return GenerateFromSeed(seed, size);
        }

        /// <summary>
        /// Generates an avatar sprite from a numeric seed.
        /// </summary>
        public static Sprite GenerateFromSeed(int seed, int size = 256) {
            Texture2D texture = GenerateTexture(seed, size);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return sprite;
        }

        static Texture2D GenerateTexture(int seed, int size) {
            Texture2D texture = new Texture2D(size, size);
            System.Random random = new System.Random(seed);

            // Generate color palette from seed
            Color backgroundColor = GenerateColor(random);
            Color primaryColor = GenerateColor(random);
            Color secondaryColor = GenerateColor(random);

            // Initialize with background
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) {
                pixels[i] = backgroundColor;
            }

            // Generate geometric pattern (grid-based identicon style)
            int gridSize = 5;
            int cellSize = size / gridSize;
            bool[,] pattern = GeneratePattern(random, gridSize);

            for (int y = 0; y < gridSize; y++) {
                for (int x = 0; x < gridSize; x++) {
                    if (pattern[x, y]) {
                        Color cellColor = (random.Next(0, 2) == 0) ? primaryColor : secondaryColor;
                        FillRect(pixels, size, x * cellSize, y * cellSize, cellSize, cellSize, cellColor);
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        static bool[,] GeneratePattern(System.Random random, int gridSize) {
            bool[,] pattern = new bool[gridSize, gridSize];
            int half = gridSize / 2;

            // Generate half pattern and mirror it for symmetry
            for (int y = 0; y < gridSize; y++) {
                for (int x = 0; x <= half; x++) {
                    bool value = random.Next(0, 2) == 1;
                    pattern[x, y] = value;
                    pattern[gridSize - 1 - x, y] = value; // Mirror
                }
            }

            return pattern;
        }

        static void FillRect(Color[] pixels, int textureWidth, int x, int y, int width, int height, Color color) {
            for (int py = y; py < y + height && py < textureWidth; py++) {
                for (int px = x; px < x + width && px < textureWidth; px++) {
                    int index = py * textureWidth + px;
                    if (index >= 0 && index < pixels.Length) {
                        pixels[index] = color;
                    }
                }
            }
        }

        static Color GenerateColor(System.Random random) {
            float hue = (float)random.NextDouble();
            float saturation = 0.5f + (float)random.NextDouble() * 0.5f;
            float value = 0.6f + (float)random.NextDouble() * 0.4f;
            return Color.HSVToRGB(hue, saturation, value);
        }

        static int GetSeedFromString(string input) {
            int hash = 0;
            foreach (char c in input) {
                hash = (hash << 5) - hash + c;
                hash = hash & hash; // Convert to 32bit integer
            }
            return hash;
        }

        /// <summary>
        /// Generates a circular avatar (with transparent corners).
        /// </summary>
        public static Sprite GenerateCircularAvatar(string input, int size = 256) {
            Sprite baseSprite = GenerateFromString(input, size);
            Texture2D texture = baseSprite.texture;

            ApplyCircularMask(texture);

            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        static void ApplyCircularMask(Texture2D texture) {
            int width = texture.width;
            int height = texture.height;
            Color[] pixels = texture.GetPixels();
            Vector2 center = new Vector2(width / 2f, height / 2f);
            float radius = width / 2f;

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    int index = y * width + x;
                    float distance = Vector2.Distance(new Vector2(x, y), center);

                    if (distance > radius) {
                        pixels[index] = Color.clear;
                    } else if (distance > radius - 2) {
                        // Soft edge
                        float alpha = (radius - distance) / 2f;
                        pixels[index] = new Color(pixels[index].r, pixels[index].g, pixels[index].b, alpha);
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
        }
    }
}
