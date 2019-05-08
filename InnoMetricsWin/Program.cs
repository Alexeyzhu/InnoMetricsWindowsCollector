using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace ConsoleApp1
{
    class Program
    {

        static void Main(string[] args)
        {

            string path = System.Environment.CurrentDirectory + @"\session.txt";
            string pathKey = System.Environment.CurrentDirectory + @"\key.pem";
            string session, password, name, lastName, email, publicKey = "";
            string[] sessionArray;
            string currentActivity = "", newActivity = "";
            string browserUrl = "";
            string executableName = "";
            bool canProceed = true;
            long startTime, endTime;
            MyHttpClient.SetUp();

            try
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.StreamReader file = new System.IO.StreamReader(path);
                    session = file.ReadLine();
                    System.IO.StreamReader fileKey = new System.IO.StreamReader(pathKey);
                    publicKey = file.ReadLine();
                }
                else
                {
                    Console.WriteLine("Please enter your credentials");
                    Console.Write("First name: ");
                    name = Console.ReadLine();
                    Console.Write("Last name: ");
                    lastName = Console.ReadLine();
                    Console.Write("Email: ");
                    email = Console.ReadLine();
                    Console.Write("Password: ");
                    password = ReadPassword();
                    Console.WriteLine();
                    
                    int HASH_SIZE = 64; // size in bytes
                    int ITERATIONS = 10000; // number of pbkdf2 iterations

                    // Generate a salt
                    RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
                    byte[] salt = Encoding.ASCII.GetBytes(email);

                    // Generate the hash
                    Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, ITERATIONS);

                    User user = new User
                    {
                        Name = name,
                        LastName = lastName,
                        Email = email,
                        Password = BitConverter.ToString(pbkdf2.GetBytes(HASH_SIZE)).Replace("-", string.Empty).ToLower()
                    };
                    sessionArray = MyHttpClient.CreateUserAsync(user).Result;
                    session = sessionArray[0];
                    publicKey = sessionArray[1];

                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine(session);
                    }

                    using (StreamWriter sws = File.CreateText(pathKey))
                    {
                        sws.WriteLine(publicKey);
                    }
                }

            }
            catch (System.Net.Http.HttpRequestException)
            {
                canProceed = false;
                session = "";
                Console.WriteLine("Unable to sign in or sign up using provided credentials");
                Console.ReadLine();
            }
            catch (AggregateException)
            {
                canProceed = false;
                session = "";
                Console.WriteLine("Unable to sign in or sign up using provided credentials");
                Console.ReadLine();
            }
            catch (Exception)
            {
                session = "";
                canProceed = false;
                Console.WriteLine("There was a mistake during read/write of session file try to run the app again");
                Console.ReadLine();
            }

            //ShowWindow(consoleHandle, SW_HIDE);

            if (canProceed)
            {
                Console.WriteLine("Now activity data is being sent to web server. You can minimize this window. \nIf you want to stop data collection/transmission close this window");
                startTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                try
                {
                    currentActivity = GetActiveWindowTitle();
                }
                catch
                {
                    Console.WriteLine("Cannot fetch current Window title");
                }


                while (true)
                {
                  try
                    {
                        browserUrl = "None";
                        executableName = "None";

                        newActivity = GetActiveWindowTitle();
                        if (!currentActivity.Equals(newActivity))
                        {
                            currentActivity = newActivity;

                            endTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

                            AesCryptoServiceProvider myAes = new AesCryptoServiceProvider();
                            myAes.Mode = System.Security.Cryptography.CipherMode.CBC;

                            RSAcust rsa = new RSAcust();

                            byte[] startTimeBytes = AesGcm256.EncryptStringToBytes_Aes(startTime.ToString(), myAes.Key, myAes.IV);
                            byte[] currectActivityBytes = AesGcm256.EncryptStringToBytes_Aes(currentActivity, myAes.Key, myAes.IV);
                            byte[] endTimeBytes = AesGcm256.EncryptStringToBytes_Aes(endTime.ToString(), myAes.Key, myAes.IV);
                            byte[] encRsaEnc = rsa.RSA_Encrypt(myAes.Key, pathKey);

                            string encKeyUn = BitConverter.ToString(encRsaEnc).Replace("-", string.Empty).ToLower();
                            string inVectorUn = BitConverter.ToString(myAes.IV).Replace("-", string.Empty).ToLower();
                            string exName = BitConverter.ToString(currectActivityBytes).Replace("-", string.Empty).ToLower();

                            Activity activity = new Activity
                            {
                                EncKey = encKeyUn,
                                IV = inVectorUn,
                                StartTime = startTime.ToString(),
                                ExecutableName = exName,
                                EndTime = endTime.ToString(),
                                BrowserUrl = "",
                                BrowserTitle = "",
                                IpAddress = "",
                                MacAddress = ""
                            };

                            if (currentActivity.EndsWith("Mozilla Firefox"))
                            {
                                browserUrl = currentActivity.Substring(0, currentActivity.Length - 18);

                                byte[] browseUrlBytes = AesGcm256.EncryptStringToBytes_Aes(browserUrl, myAes.Key, myAes.IV);
                                activity.BrowserTitle = BitConverter.ToString(browseUrlBytes).Replace("-", string.Empty).ToLower();
                                activity.BrowserUrl = BitConverter.ToString(browseUrlBytes).Replace("-", string.Empty).ToLower();

                                executableName = "Mozilla Firefox";

                                byte[] executableNameBytes = AesGcm256.EncryptStringToBytes_Aes(executableName, myAes.Key, myAes.IV);
                                activity.ExecutableName = BitConverter.ToString(executableNameBytes).Replace("-", string.Empty).ToLower();
                            }
                            else if (currentActivity.EndsWith("Google Chrome"))
                            {
                                browserUrl = currentActivity.Substring(0, currentActivity.Length - 16);

                                byte[] browseUrlBytes = AesGcm256.EncryptStringToBytes_Aes(browserUrl, myAes.Key, myAes.IV);
                                activity.BrowserTitle = BitConverter.ToString(browseUrlBytes).Replace("-", string.Empty).ToLower();
                                activity.BrowserUrl = BitConverter.ToString(browseUrlBytes).Replace("-", string.Empty).ToLower();

                                executableName = "Google Chrome";

                                byte[] executableNameBytes = AesGcm256.EncryptStringToBytes_Aes(executableName, myAes.Key, myAes.IV);
                                activity.ExecutableName = BitConverter.ToString(executableNameBytes).Replace("-", string.Empty).ToLower();
                            }
                            else if (currentActivity.EndsWith("Microsoft Edge"))
                            {
                                browserUrl = currentActivity.Substring(0, currentActivity.Length - 17);

                                byte[] browseUrlBytes = AesGcm256.EncryptStringToBytes_Aes(browserUrl, myAes.Key, myAes.IV);
                                activity.BrowserTitle = BitConverter.ToString(browseUrlBytes).Replace("-", string.Empty).ToLower();
                                activity.BrowserUrl = BitConverter.ToString(browseUrlBytes).Replace("-", string.Empty).ToLower();

                                executableName = "Microsoft Edge";

                                byte[] executableNameBytes = AesGcm256.EncryptStringToBytes_Aes(executableName, myAes.Key, myAes.IV);
                                activity.ExecutableName = BitConverter.ToString(executableNameBytes).Replace("-", string.Empty).ToLower();
                            }

                            startTime = endTime;
                            
                            MyHttpClient.SendActivityAsync(activity, session);

                        }
                        System.Threading.Thread.Sleep(1000);
                    }
                    catch
                    {
                       Console.WriteLine("Failed to gather the information");
                    }
                }
            }
        }



        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return "untitled activity";
        }

        private static string ReadPassword()
        {
            string pass = "";
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            } while (true);

            return pass;
        }
    }

}
