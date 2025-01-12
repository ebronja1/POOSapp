using System;
using System.Drawing;
using System.IO;
using System.Linq;
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
                // Load the model into a graph
                var graph = new Graph().as_default();
                graph.Import(_modelPath); // Import the saved model graph

                // Start a session
                using (var session = new Session(graph))
                {
                    // Preprocess the image (resize and normalize)
                    Tensor inputTensor = PreprocessImage(imageBytes);

                    // Get the input and output tensor names from the graph
                    var inputOp = graph.OperationByName("input_1");  // Change "input_1" to the actual input tensor name
                    var outputOp = graph.OperationByName("output_0");  // Change "output_0" to the actual output tensor name

                    // Run inference
                    var result = session.run(outputOp.outputs[0], new FeedItem(inputOp, inputTensor));

                    // Convert result to an array and find the predicted label
                    var predictions = result.ToArray<float>();
                    var predictedLabel = Array.IndexOf(predictions, predictions.Max());
                    var confidence = predictions.Max();

                    return $"Predicted label: {predictedLabel}, Confidence: {confidence}";
                }
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
