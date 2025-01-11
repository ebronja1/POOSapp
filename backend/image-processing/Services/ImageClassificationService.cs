using System.Drawing;
using Tensorflow;
using Tensorflow.NumPy;
using static Tensorflow.Binding;

namespace ImageClassifierAPI.Services
{
    public class ImageClassificationService
    {
        private readonly string _modelPath;

        public ImageClassificationService(string modelPath)
        {
            _modelPath = modelPath;
        }

        public string ClassifyImage(byte[] imageBytes)
        {
            try
            {
                // Load the SavedModel
                var model = tf.saved_model.load(_modelPath);

                // Preprocess the image (resize and normalize)
                Tensor inputTensor = PreprocessImage(imageBytes);

                // Retrieve the serving_default function
                var predictFunction = model.signatures["serving_default"];

                // Run inference
                var output = predictFunction.call(new FeedItem[]
                {
                    new FeedItem("input_1", inputTensor) // Adjust "input_1" to match your model's input tensor name
                });

                // Extract predictions (update output tensor name as needed)
                var predictions = output["output_0"].ToArray<float>(); // Adjust "output_0" for your model's output tensor name
                var predictedLabel = Array.IndexOf(predictions, predictions.Max());
                var confidence = predictions.Max();

                return $"Predicted label: {predictedLabel}, Confidence: {confidence}";
            }
            catch (Exception ex)
            {
                return $"Error during classification: {ex.Message}";
            }
        }

        private Tensor PreprocessImage(byte[] imageBytes)
        {
            // Resize and normalize the image
            var bitmap = new Bitmap(new MemoryStream(imageBytes));
            var resizedBitmap = new Bitmap(bitmap, new Size(224, 224)); // Resize to match model input size
            var data = new float[224, 224, 3];

            for (int i = 0; i < resizedBitmap.Width; i++)
            {
                for (int j = 0; j < resizedBitmap.Height; j++)
                {
                    var pixel = resizedBitmap.GetPixel(i, j);
                    data[i, j, 0] = pixel.R / 255.0f;
                    data[i, j, 1] = pixel.G / 255.0f;
                    data[i, j, 2] = pixel.B / 255.0f;
                }
            }

            return tf.expand_dims(tf.convert_to_tensor(data), 0); // Add batch dimension
        }
    }
}
