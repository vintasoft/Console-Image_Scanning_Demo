using System;
using Vintasoft.Primitives;
using Vintasoft.UniversalImageScanner;

namespace ImageScanningConsoleDemo
{
    class Program
    {

        static void Main(string[] args)
        {
            // register the evaluation license for VintaSoft Twain .NET SDK
            Vintasoft.Twain.TwainGlobalSettings.Register("REG_USER", "REG_EMAIL", "EXPIRATION_DATE", "REG_CODE");
            Vintasoft.WiaImageScanning.WiaGlobalSettings.Register("REG_USER", "REG_EMAIL", "EXPIRATION_DATE", "REG_CODE");
            Vintasoft.Sane.SaneGlobalSettings.Register("REG_USER", "REG_EMAIL", "EXPIRATION_DATE", "REG_CODE");
            Vintasoft.EsclImageScanning.EsclGlobalSettings.Register("REG_USER", "REG_EMAIL", "EXPIRATION_DATE", "REG_CODE");

            try
            {
                // create the universal image scanner manager
                using (UniversalImageScannerManager imageScannerManager = new UniversalImageScannerManager())
                {
                    // open the universal image scanner manager
                    imageScannerManager.Open();

                    // select the universal image scanner
                    IUniversalImageScanner imageScanner = SelectUniversalImageScanner(imageScannerManager);
                    // if device is not selected
                    if (imageScanner == null)
                        return;

                    // specify that device should not show UI
                    imageScanner.ShowUI = false;

                    // open the universal image scanner
                    imageScanner.Open();

                    // output name of the universal image scanner
                    Console.WriteLine(string.Format("Device name: {0}", imageScanner.Name));
                    Console.WriteLine();

                    // select the scan input source for the universal image scanner
                    SelectInputSourceForUniversalImageScanner(imageScanner);

                    // select the image pixel type for scanning images
                    ImagePixelType? imagePixelType = SelectImagePixelType();
                    // if image pixel type is selected
                    if (imagePixelType.HasValue)
                    {
                        try
                        {
                            // specify the image pixel type for scanning images
                            imageScanner.ImagePixelType = imagePixelType.Value;
                            Console.WriteLine("The image pixel type is set.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format("Set image pixel type error: {0}", ex.Message));
                        }
                    }
                    Console.WriteLine();

                    // select the image resolution for scanning images
                    int? imageResolution = SelectImageResolution();
                    // if image resolution is selected
                    if (imageResolution.HasValue)
                    {
                        try
                        {
                            // specify that images must be scanned with selected resolution
                            imageScanner.ScanXResolution = imageResolution.Value;
                            imageScanner.ScanYResolution = imageResolution.Value;
                            Console.WriteLine("The image resolution is set.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format("Set image resolution error: {0}", ex.Message));
                        }
                    }
                    Console.WriteLine();

                    // select the page size for scanning images
                    VintasoftRect? pageSize = SelectPageSize();
                    // if image resolution is selected
                    if (pageSize.HasValue)
                    {
                        try
                        {
                            // set the page size
                            imageScanner.SetImageLayout(pageSize.Value);
                            Console.WriteLine("The page size is set.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format("Set page size error: {0}", ex.Message));
                        }
                    }
                    Console.WriteLine();

                    // select the name of image encoder for saving acquired images
                    string imageEncoderName = SelectImageEncoder();
                    if (imageEncoderName == null)
                        imageEncoderName = "png";
                    Console.WriteLine();


                    Console.WriteLine("Images acquisition is started...");
                    int imageIndex = 0;
                    string filename;
                    do
                    {
                        try
                        {
                            // if image is acquired from image scanner
                            if (imageScanner.AcquireImageSync())
                            {
                                Console.WriteLine("Image is acquired.");

                                if (imageEncoderName == "tif" || imageEncoderName == "pdf")
                                    filename = string.Format("scannedImage.{0}", imageEncoderName);
                                else
                                    filename = string.Format("scannedImage{0}.{1}", imageIndex, imageEncoderName);

                                // save acquired image to a file
                                imageScanner.SaveImage(imageIndex, filename);

                                Console.WriteLine(string.Format("Image{0} is saved.", imageIndex++));
                            }
                            // if image is not acquired
                            else
                            {
                                Console.WriteLine("Scan is finished.");
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format("Scan is failed: {0}", ex.Message));
                            break;
                        }
                    }
                    while (true);

                    // close the universal image scanner
                    imageScanner.Close();

                    // close the universal image scanner manager
                    imageScannerManager.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + GetFullExceptionMessage(ex));
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Selects the universal image scanner.
        /// </summary>
        /// <param name="universalImageScannerManager">The universal image scanner manager.</param>
        /// <returns>The universal image scanner if scanner is selected; otherwise, <b>null</b>.</returns>
        private static IUniversalImageScanner SelectUniversalImageScanner(UniversalImageScannerManager universalImageScannerManager)
        {
            // get the names of available image scanners
            string[] deviceNames = universalImageScannerManager.GetUniversalImageScannerNames();
            // if image scanners are not found in the system
            if (deviceNames.Length == 0)
            {
                Console.WriteLine("Devices are not found.");
                return null;
            }

            Console.WriteLine("Device list:");
            // for each image scanner
            for (int i = 0; i < deviceNames.Length; i++)
            {
                // display the name of image scanner
                Console.WriteLine(string.Format("{0}. {1}", i + 1, deviceNames[i]));
            }

            int deviceIndex = -1;
            while (deviceIndex < 0 || deviceIndex > deviceNames.Length)
            {
                Console.Write(string.Format("Please select device by entering the device number from '1' to '{0}' ('0' to cancel) and press 'Enter' key: ", deviceNames.Length));
                string deviceIndexString = Console.ReadLine();
                int.TryParse(deviceIndexString, out deviceIndex);
            }
            Console.WriteLine();

            if (deviceIndex == 0)
                return null;

            return universalImageScannerManager.Find(deviceNames[deviceIndex - 1]);
        }

        /// <summary>
        /// Selects the scan input source for the universal image scanner.
        /// </summary>
        /// <param name="universalImageScanner">The universal image scanner.</param>
        private static void SelectInputSourceForUniversalImageScanner(IUniversalImageScanner universalImageScanner)
        {
            // if the universal image scanner has flatbed and feeder
            if (universalImageScanner.HasFlatbed && universalImageScanner.HasFeeder)
            {
                if (universalImageScanner.HasDuplex)
                    Console.Write("Device has flatbed and feeder with duplex.");
                else
                    Console.Write("Device has flatbed and feeder without duplex.");

                if (universalImageScanner.IsFeederEnabled)
                    Console.WriteLine(" Now device uses feeder.");
                else
                    Console.WriteLine(" Now device uses flatbed.");


                // ask user to select flatbed or feeder

                int scanInputSourceIndex = -1;
                while (scanInputSourceIndex != 1 && scanInputSourceIndex != 2)
                {
                    Console.Write("What do you want to use: flatbed (press '1') or feeder (press '2'): ");
                    scanInputSourceIndex = Console.ReadKey().KeyChar - '0';
                    Console.WriteLine();
                }
                Console.WriteLine();

                if (scanInputSourceIndex == 1)
                    universalImageScanner.IsFlatbedEnabled = true;
                else
                    universalImageScanner.IsFeederEnabled = true;
            }
            // if the universal image scanner has feeder only
            else if (!universalImageScanner.HasFlatbed && universalImageScanner.HasFeeder)
            {
                if (universalImageScanner.HasDuplex)
                    Console.WriteLine("Device has feeder with duplex.");
                else
                    Console.WriteLine("Device has feeder without duplex.");
                Console.WriteLine();
            }
            // if the universal image scanner has flatbed only
            else if (universalImageScanner.HasFlatbed && !universalImageScanner.HasFeeder)
            {
                Console.WriteLine("Device has flatbed only.");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Selects the image pixel type for the universal image scanner.
        /// </summary>
        /// <returns>null if image pixel type is not selected; otherwise, selected image pixel type.</returns>
        private static ImagePixelType? SelectImagePixelType()
        {
            Console.WriteLine("Image pixel types:");
            Console.WriteLine("1. Black-white");
            Console.WriteLine("2. Grayscale");
            Console.WriteLine("3. RGB");

            int pixelTypeIndex = -1;
            while (pixelTypeIndex < 0 || pixelTypeIndex > 3)
            {
                Console.Write("Please select image pixel type by entering the number from '1' to '3' or press '0' to cancel: ");
                pixelTypeIndex = Console.ReadKey().KeyChar - '0';
                Console.WriteLine();
            }
            Console.WriteLine();

            switch (pixelTypeIndex)
            {
                case 1:
                    return ImagePixelType.BlackWhite;
                case 2:
                    return ImagePixelType.Grayscale;
                case 3:
                    return ImagePixelType.RGB;
            }
            return null;
        }

        /// <summary>
        /// Selects the image resolution for the universal image scanner.
        /// </summary>
        /// <returns>null if image resolution is not selected; otherwise, selected image resolution.</returns>
        private static int? SelectImageResolution()
        {
            Console.WriteLine("Image resolutions:");
            Console.WriteLine("1. 200 dpi");
            Console.WriteLine("2. 300 dpi");
            Console.WriteLine("3. 600 dpi");

            int imageResolutionIndex = -1;
            while (imageResolutionIndex < 0 || imageResolutionIndex > 3)
            {
                Console.Write("Please select the image resolution by entering the number from '1' to '3' or press '0' to cancel: ");
                imageResolutionIndex = Console.ReadKey().KeyChar - '0';
                Console.WriteLine();
            }
            Console.WriteLine();

            switch (imageResolutionIndex)
            {
                case 1:
                    return 200;
                case 2:
                    return 300;
                case 3:
                    return 600;
            }
            return null;
        }

        /// <summary>
        /// Selects the page size for the universal image scanner.
        /// </summary>
        /// <returns>null if page size is not selected; otherwise, selected page size.</returns>
        private static VintasoftRect? SelectPageSize()
        {
            Console.WriteLine("Page sizes:");
            Console.WriteLine("1. US Letter (8.5 x 11 inches)");
            Console.WriteLine("2. A5 (5.8 x 8.2 inches)");
            Console.WriteLine("3. A4 (8.2 x 11.6 inches)");
            Console.WriteLine("4. B5 dpi (6.9 x 9.8 inches)");

            int pageSizeIndex = -1;
            while (pageSizeIndex < 0 || pageSizeIndex > 4)
            {
                Console.Write("Please select the page size by entering the number from '1' to '4' or press '0' to cancel: ");
                pageSizeIndex = Console.ReadKey().KeyChar - '0';
                Console.WriteLine();
            }
            Console.WriteLine();

            switch (pageSizeIndex)
            {
                case 1:
                    return new VintasoftRect(0, 0, 8.5, 11);
                case 2:
                    return new VintasoftRect(0, 0, 5.8, 8.2);
                case 3:
                    return new VintasoftRect(0, 0, 8.2, 11.6);
                case 4:
                    return new VintasoftRect(0, 0, 6.9, 9.8);
            }
            return null;
        }

        /// <summary>
        /// Selects the page size for the universal image scanner.
        /// </summary>
        /// <returns>null if page size is not selected; otherwise, selected page size.</returns>
        private static string SelectImageEncoder()
        {
            Console.WriteLine("Image encoders for saving acquired images:");
            Console.WriteLine("1. PNG");
            Console.WriteLine("2. JPEG");
            Console.WriteLine("3. TIFF");
            Console.WriteLine("4. PDF");
            Console.WriteLine("5. BMP");

            int imageEncoderIndex = -1;
            while (imageEncoderIndex < 0 || imageEncoderIndex > 5)
            {
                Console.Write("Please select the image encoder by entering the number from '1' to '5' or press '0' to cancel: ");
                imageEncoderIndex = Console.ReadKey().KeyChar - '0';
                Console.WriteLine();
            }
            Console.WriteLine();

            switch (imageEncoderIndex)
            {
                case 1:
                    return "png";
                case 2:
                    return "jpg";
                case 3:
                    return "tif";
                case 4:
                    return "pdf";
                case 5:
                    return "bmp";
            }
            return null;
        }

        /// <summary>
        /// Returns message that contains information about exception and inner exceptions.
        /// </summary>
        private static string GetFullExceptionMessage(Exception ex)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine(ex.Message);

            Exception innerException = ex.InnerException;
            while (innerException != null)
            {
                if (ex.Message != innerException.Message)
                    sb.AppendLine(string.Format("Inner exception: {0}", innerException.Message));
                innerException = innerException.InnerException;
            }

            return sb.ToString();
        }

    }
}
