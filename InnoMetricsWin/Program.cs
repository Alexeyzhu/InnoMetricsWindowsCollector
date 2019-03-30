using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ConsoleApp1
{
    class Program
    {

        static void Main(string[] args)
        {

            string path = System.Environment.CurrentDirectory + @"\session.txt";
            string session, password, name, lastName, email;
            string currentActivity = "", newActivity = "";
            string browserUrl = "";
            bool canProceed = true;
            long startTime, endTime;
            MyHttpClient.SetUp();

            try
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.StreamReader file = new System.IO.StreamReader(path);
                    session = file.ReadLine();
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
                    //Console.WriteLine("The password was " + password);
                    Console.WriteLine();
                    User user = new User
                    {
                        Name = name,
                        LastName = lastName,
                        Email = email,
                        Password = password
                    };
                    session = MyHttpClient.CreateUserAsync(user).Result;
                    System.IO.File.WriteAllText(path, session);
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

                }

                while (true)
                {
                    try
                    {
                        browserUrl = "";
                        //Console.WriteLine(GetActiveWindowTitle());
                        newActivity = GetActiveWindowTitle();
                        if (!currentActivity.Equals(newActivity))
                        {
                            currentActivity = newActivity;
                            endTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                            //Console.WriteLine(currentActivity);
                            Activity activity = new Activity
                            {
                                StartTime = startTime.ToString(),
                                ExecutableName = currentActivity,
                                EndTime = endTime.ToString(),
                                BrowserUrl = "",
                                BrowserTitle = "",
                                IpAddress = "",
                                MacAddress = ""
                            };


                            if (currentActivity.EndsWith("Mozilla Firefox"))
                            {
                                browserUrl = currentActivity.Substring(0, currentActivity.Length - 18);
                                activity.BrowserTitle = browserUrl;
                                activity.ExecutableName = "Mozilla Firefox";
                                activity.BrowserUrl = browserUrl;

                            }
                            else if (currentActivity.EndsWith("Google Chrome"))
                            {
                                browserUrl = currentActivity.Substring(0, currentActivity.Length - 16);
                                activity.BrowserTitle = browserUrl;
                                activity.ExecutableName = "Google Chrome";
                                activity.BrowserUrl = browserUrl;

                            }
                            else if (currentActivity.EndsWith("Microsoft Edge"))
                            {
                                browserUrl = currentActivity.Substring(0, currentActivity.Length - 17);
                                activity.BrowserTitle = browserUrl;
                                activity.ExecutableName = "Microsoft Edge";
                                activity.BrowserUrl = browserUrl;

                            }
                            startTime = endTime;
                            //string logPath = System.Environment.CurrentDirectory + @"\log.txt";
                            //System.IO.File.WriteAllText(path, activity.ExecutableName);
                            //Console.WriteLine(activity.ExecutableName);
                            MyHttpClient.SendActivityAsync(activity, session);

                        }
                        System.Threading.Thread.Sleep(1000);
                    }
                    catch
                    {

                    }
                }
            }
        }



        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
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
