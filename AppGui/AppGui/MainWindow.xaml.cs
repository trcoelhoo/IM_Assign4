using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;
using OpenQA.Selenium;
using prmToolkit.Selenium;
using prmToolkit.Selenium.Enum;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

using System.Speech.Synthesis;
using System.Speech.AudioFormat;

namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        

        private MmiCommunication mmiC;

        //  new 16 april 2020
        private MmiCommunication mmiSender;
        private LifeCycleEvents lce;
        private MmiCommunication mmic;
       
        public static class WebDriver
        {

             
            
            public static IWebDriver webDriver;
            public static void InitializeDriver()
            {
                string localDriver = System.AppDomain.CurrentDomain.BaseDirectory.ToString();
                ChromeOptions options = new ChromeOptions();
                options.AddArgument("--start-maximized");
                options.AddArgument("--disable-notifications");
                WebDriver.webDriver = new ChromeDriver(localDriver + "/driver", options);
                
                
            }

            public static IWebDriver GetWebDriver()
            {
                return WebDriver.webDriver;
            }
        }

        public static class AppContext
        {
            public static String context;
            public static Boolean nextPage;
            public static int cont;

            public static void ChangeContext(String c) 
            {
                AppContext.context = c;
            }

            public static String GetContext()
            {
                return AppContext.context;
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            WebDriver.InitializeDriver();
            synthesizer.SetOutputToDefaultAudioDevice();
            var culture = new System.Globalization.CultureInfo("pt-PT");
            var voices = synthesizer.GetInstalledVoices(culture);
            AppContext.cont = 0;
            synthesizer.SelectVoice(voices[0].VoiceInfo.Name);
            
            AppContext.context = "";
            AppContext.nextPage = false;

            mmiC = new MmiCommunication("localhost", 8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

            // NEW 16 april 2020
            //init LifeCycleEvents..
            lce = new LifeCycleEvents("APP", "TTS", "User1", "na", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode
            // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)
            mmic = new MmiCommunication("localhost", 8000, "User1", "GUI");



        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);

            var com = doc.Descendants("command").FirstOrDefault().Value;
            Console.WriteLine(com.ToString());
            Console.WriteLine(AppContext.context);
            dynamic json = JsonConvert.DeserializeObject(com);

            Console.WriteLine("recebeu "+(string)json.recognized.ToString());
            switch ((string)json.recognized[0].ToString())
            {
               
                case "ABRIR":
                    try
                    {

                        if (AppContext.context == "")
                        {
                            Console.WriteLine("SELENIUM ABRIR NETFLIX");
                            synthesizer.Speak("Bem-vindo. A iniciar a néteflix.");

                            WebDriver.webDriver.Navigate().GoToUrl("https://www.netflix.com/pt/");
                            System.Threading.Thread.Sleep(500);
                            Wait(WebDriver.webDriver, By.XPath("/html/body/div[1]/div/div/div/div/div/div[1]/div[1]/div/div[1]/button[1]"));

                            WebDriver.webDriver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div[1]/div[1]/div/div[1]/button[1]")).Click();

                            Wait(WebDriver.webDriver, By.ClassName("authLinks"));
                            WebDriver.webDriver.FindElement(By.ClassName("authLinks")).Click();
                            Login(WebDriver.webDriver, "mail", "pass", "code");
                            AppContext.context = "HOME";
                        }

                        
                    }
                    catch
                    {

                    }
                    break;


                case "PESQUISAR":
                    try
                    {
                        if (AppContext.context == "HOME"|| AppContext.context == "SEARCH"|| AppContext.context == "IDIOMA"||AppContext.context == "NFinished")
                        {
                            AppContext.cont = 0;
                            // pesquisar o tipo de filme
                            searchTypeOfFilm(json.recognized[1].ToString());
                            
                            
                        }
                        
                    } catch
                    {
                        
                    }
                    break;
                case "NFinished":
                    try
                    {
                        if (AppContext.context == "HOME" || AppContext.context == "SEARCH" || AppContext.context == "IDIOMA" || AppContext.context == "NFinished")
                        {
                            AppContext.cont = 0;
                            // pesquisar conteudo nao acabado
                            ShowNotFinished(WebDriver.webDriver);
                            FirstContentNFinished(WebDriver.webDriver);
                            synthesizer.Speak("Estes são os conteúdos que não acabáste de ver.");
                            AppContext.context = "NFinished";

                        }

                    }
                    catch
                    {

                    }
                    break;

                case "SLIDEL":
                    try
                    {

                        if (AppContext.context == "SEARCH" || AppContext.context == "IDIOMA" || AppContext.context == "NFinished")
                        {
                            if (AppContext.cont > 0)
                            {

                                LeftContent(WebDriver.webDriver);
                                AppContext.cont -= 1;
                            }
                            else
                            {
                                synthesizer.Speak("Não é possível ir para o conteúdo à esquerda");
                            }
                            
                        }
                        
                    }
                    catch
                    {

                    }
                    break;
                case "SLIDER":
                    try
                    {
                        if (AppContext.context == "SEARCH" || AppContext.context == "IDIOMA" || AppContext.context == "NFinished")
                        {
                            Console.WriteLine(AppContext.cont);
                            RightContent(WebDriver.webDriver);
                            AppContext.cont += 1;
                        }
                    }
                    catch
                    {

                    }
                    break;
                case "SLIDEUP":
                    try
                    {
                        if (AppContext.context == "SEARCH" || AppContext.context == "IDIOMA"  )
                        {
                            if (AppContext.cont > 5)
                            {
                                UpContent(WebDriver.webDriver);
                                AppContext.cont -= 5;

                            }
                            else
                            {
                                Console.WriteLine(AppContext.cont);
                                synthesizer.Speak("Não é possível ir para o conteúdo em cima");
                            }
                            
                        }
                        else if(AppContext.context == "NFinished")
                        {
                            if (AppContext.cont > 4)
                            {
                                UpContentNFinished(WebDriver.webDriver);
                                AppContext.cont -= 5;

                            }
                            else
                            {
                                Console.WriteLine(AppContext.cont);
                                synthesizer.Speak("Não é possível ir para o conteúdo em cima");
                            }
                        }
                    }
                    catch
                    {

                    }
                    break;
                case "SLIDEDOWN":
                    try
                    {
                        if (AppContext.context == "SEARCH" || AppContext.context == "IDIOMA"  )
                        {
                            DownContent(WebDriver.webDriver);
                            AppContext.cont += 6;
                        }
                        else if (AppContext.context == "NFinished")
                        {
                            DownContentNFinished(WebDriver.webDriver);
                            AppContext.cont += 5;
                        }
                    }
                    catch
                    {

                    }
                    break;
                case "SELECT":
                    try
                    {   
                        if (json.recognized[1].ToString() == "GESTOS")
                        {
                            if (AppContext.context == "SEARCH")
                            {
                                AppContext.cont = 0;

                                SelecContGestos(WebDriver.webDriver);

                                AppContext.context = "INCONT";

                            }
                            else if (AppContext.context == "IDIOMA")
                            {
                                AppContext.cont = 0;

                                SelecContGestos(WebDriver.webDriver);

                                AppContext.context = "INCONTIDIOMA";
                            }
                            else if (AppContext.context == "NFinished")
                            {
                                AppContext.cont = 0;

                                SelecContGestos(WebDriver.webDriver);
                                AppContext.context = "INCONTNFinished";
                            }

                            else if (AppContext.context == "INCONT")
                            {

                                ReproduzirCont(WebDriver.webDriver);
                                synthesizer.Speak("Boa escolha. Aproveite a sessão!");
                                AppContext.context = "REP";
                            }
                            else if (AppContext.context == "INCONTIDIOMA")
                            {

                                ReproduzirCont(WebDriver.webDriver);
                                synthesizer.Speak("Boa escolha. Aproveite a sessão!");
                                AppContext.context = "REPIDIOMA";
                            }
                            else if (AppContext.context == "INCONTNFinished")
                            {

                                ReproduzirCont(WebDriver.webDriver);
                                synthesizer.Speak("Boa escolha. Aproveite a sessão!");
                                AppContext.context = "REPNFinished";
                            }
                        }
                        else
                        {
                            if (AppContext.context == "SEARCH")

                            {
                                AppContext.cont = 0;
                                SelecCont(WebDriver.webDriver, json.recognized[2].ToString(), json.recognized[1].ToString());
                                AppContext.context = "INCONT";
                            }
                            else if (AppContext.context == "IDIOMA")
                            {
                                AppContext.cont = 0;
                                SelectContIdioma(WebDriver.webDriver, json.recognized[2].ToString(), json.recognized[1].ToString());
                                AppContext.context = "INCONTIDIOMA";
                            }
                            else if (AppContext.context == "NFinished")
                            {
                                AppContext.cont = 0;
                                SelectNotFinished(WebDriver.webDriver, json.recognized[2].ToString(), json.recognized[1].ToString());
                                AppContext.context = "INCONTNFinished";
                            }
                        }
                        
                    }
                    catch
                    {

                    }
                    break;
                case "GOSTO":
                    try
                    {
                        if (json.recognized[1].ToString() == "GESTOS")
                        {
                            if (AppContext.context == "SEARCH")
                            {
                                AppContext.cont = 0;

                                SelecContGestos(WebDriver.webDriver);

                                AppContext.context = "INCONT";

                            }
                            else if (AppContext.context == "IDIOMA")
                            {
                                AppContext.cont = 0;

                                SelecContGestos(WebDriver.webDriver);

                                AppContext.context = "INCONTIDIOMA";
                            }
                            else if (AppContext.context == "NFinished")
                            {
                                AppContext.cont = 0;

                                SelecContGestos(WebDriver.webDriver);
                                AppContext.context = "INCONTNFinished";
                            }

                            else if (AppContext.context == "INCONT")
                            {

                                ReproduzirCont(WebDriver.webDriver);
                                synthesizer.Speak("Boa escolha. Aproveite a sessão!");
                                AppContext.context = "REP";
                            }
                            else if (AppContext.context == "INCONTIDIOMA")
                            {

                                ReproduzirCont(WebDriver.webDriver);
                                synthesizer.Speak("Boa escolha. Aproveite a sessão!");
                                AppContext.context = "REPIDIOMA";
                            }
                            else if (AppContext.context == "INCONTNFinished")
                            {

                                ReproduzirCont(WebDriver.webDriver);
                                synthesizer.Speak("Boa escolha. Aproveite a sessão!");
                                AppContext.context = "REPNFinished";
                            }
                        }
                        else
                        {
                            if (AppContext.context == "SEARCH")

                            {
                                AppContext.cont = 0;
                                SelecCont(WebDriver.webDriver, json.recognized[2].ToString(), json.recognized[1].ToString());
                                AppContext.context = "INCONT";
                            }
                            else if (AppContext.context == "IDIOMA")
                            {
                                AppContext.cont = 0;
                                SelectContIdioma(WebDriver.webDriver, json.recognized[2].ToString(), json.recognized[1].ToString());
                                AppContext.context = "INCONTIDIOMA";
                            }
                            else if (AppContext.context == "NFinished")
                            {
                                AppContext.cont = 0;
                                SelectNotFinished(WebDriver.webDriver, json.recognized[2].ToString(), json.recognized[1].ToString());
                                AppContext.context = "INCONTNFinished";
                            }
                        }

                    }
                    catch
                    {

                    }
                    break;
                case "CANCEL":
                    try
                    {
                        if (AppContext.context == "INCONT")
                        {
                            
                            FecharCont(WebDriver.webDriver);
                            RefreshPage(WebDriver.webDriver);
                            FirstContent(WebDriver.webDriver);
                            synthesizer.Speak("Pode escolher outro.");
                            AppContext.context = "SEARCH";
                        }
                        else if (AppContext.context == "INCONTIDIOMA")
                        {
                            FecharCont(WebDriver.webDriver);
                            RefreshPage(WebDriver.webDriver);
                            FirstContentIdioma(WebDriver.webDriver);
                            synthesizer.Speak("Pode escolher outro.");
                            AppContext.context = "IDIOMA";
                        }
                        else if(AppContext.context == "INCONTNFinished")
                        {
                            ShowNotFinished(WebDriver.webDriver);
                            FirstContentNFinished(WebDriver.webDriver);
                            synthesizer.Speak("Pode escolher outro.");
                            AppContext.context = "NFinished";
                        }
                    }
                    catch { }
                    break;
                case "START":
                    try
                    {
                        if (AppContext.context == "INCONT")
                        {
                            ReproduzirCont(WebDriver.webDriver);
                            synthesizer.Speak("Boa escolha. Aproveite a sessão!");
                            AppContext.context = "REP";
                        }
                        else if (AppContext.context == "INCONTIDIOMA")
                        {
                            ReproduzirCont(WebDriver.webDriver);
                            synthesizer.Speak("Boa escolha. Aproveite a sessão!");
                            AppContext.context = "REPIDIOMA";
                        }
                        else if (AppContext.context == "INCONTNFinished")
                        {
                            ReproduzirCont(WebDriver.webDriver);
                            synthesizer.Speak("Boa escolha. Aproveite a sessão!");
                            AppContext.context = "REPNFinished";
                        }

                    }
                    catch
                    {

                    }
                    
                    break;
                case "PLAY":
                    try
                    {
                        Play(WebDriver.webDriver);
                    }
                    catch
                    {

                    }
                    
                    break;
                case "PAUSE":
                    try
                    {
                        Pause(WebDriver.webDriver);
                    }
                    catch
                    {

                    }
                    
                    break;
                case "UP":
                    try
                    {
                        AumentarSom(WebDriver.webDriver);
                    }
                    catch
                    {

                    }
                    
                    break;
                case "DOWN":
                    try
                    {
                        DiminuirSom(WebDriver.webDriver);
                    }
                    catch
                    {

                    }
                    
                    break;
                case "MUTE":
                    try
                    {
                        Mutar(WebDriver.webDriver);
                    }
                    catch
                    {

                    }
                    
                    break;
                case "UNMUTE":
                    try
                    {
                        Desmutar(WebDriver.webDriver);
                    }
                    catch
                    {

                    }
                    
                    break;
                case "JUMPINTRO":
                    try
                    {
                        IgnorarIntro(WebDriver.webDriver);
                    }
                    catch
                    {

                    }
                    
                    break;
                case "JUMP10":
                    try
                    {
                        Saltar10(WebDriver.webDriver);
                    }
                    catch
                    {

                    }
                    
                    break;
                case "JUMPBACK10":
                    try
                    {
                        Retroceder10(WebDriver.webDriver);
                    }
                    catch
                    {

                    }
                    
                    break;
                case "BACK":
                    try
                    {
                        if (AppContext.context == "REP")
                        {
                            SairCont_reproduzir(WebDriver.webDriver);
                            AppContext.context = "INCONT";
                        }
                        else if (AppContext.context == "REPIDIOMA")
                        {
                            SairCont_reproduzir(WebDriver.webDriver);
                            AppContext.context = "INCONTIDIOMA";
                        }
                        else if (AppContext.context == "REPNFinished")
                        {
                            SairCont_reproduzir(WebDriver.webDriver);
                            AppContext.context = "INCONTNFinished";
                        }

                    }
                    catch
                    {

                    }
                    
                    break;
                case "RELOAD":
                    try
                    {
                        RefreshPage(WebDriver.webDriver);
                    }
                    catch
                    {

                    }
                    
                    break;
                case "BACKPAGE":
                    try
                    {
                        GoBackPage(WebDriver.webDriver);
                    }
                    catch
                    {

                    }
                    
                    break;
                case "MOREINFO":
                    try
                    {
                        if (AppContext.context == "")
                        {
                            synthesizer.Speak("Para iniciar a néteflix, tens de dizer, abre,  a  néteflix. Coloca os credênciais dizendo, coloca os dados. Com a aplicação aberta podes, pesquisar por categorias ou idioma, podes ver o conteúdo que não acabáste, e escolher um conteúdo. Podes ainda controlar o volume, parar o vídeo e saltar 10 segundos.");
                        }
                            
                    }
                    catch
                    {

                    }
                    break;

            }


            //  new 16 april 2020
            mmic.Send(lce.NewContextRequest());

            string json2 = "{ \"synthesize\": [\"";


            Console.WriteLine((string)json.recognized[0].ToString());
            Console.WriteLine(json.recognized);

            try
            {
                json2 += (string)json.recognized[0].ToString() + " ";
                json2 += (string)json.recognized[1].ToString() + " ";
                json2 += (string)json.recognized[2].ToString() + " DONE.";
            }
            catch
            {
                json2 += "DONE.";

            }
            json2 += "\"] }";

            Console.WriteLine(json2);
            /*
             foreach (var resultSemantic in e.Result.Semantics)
            {
                json += "\"" + resultSemantic.Value.Value + "\", ";
            }
            json = json.Substring(0, json.Length - 2);
            json += "] }";
            */
            var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, json2);
            mmic.Send(exNot);


        }
        private void SelecContGestos(IWebDriver webDriver)
        {
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.Enter);
            action.Build().Perform();
        }
        private void FirstContent(IWebDriver webDriver)
        {
            Wait(webDriver, By.ClassName("search"));
            webDriver.FindElement(By.ClassName("search"));
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);


            action.Build().Perform();
        }
        private void FirstContentIdioma(IWebDriver webDriver)
        {
            Wait(webDriver, By.ClassName("logo"));
            webDriver.FindElement(By.ClassName("logo"));
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);




            action.Build().Perform();
        }
        private void FirstContentCancelIdioma(IWebDriver webDriver)
        {
            Wait(webDriver, By.ClassName("logo"));
            webDriver.FindElement(By.ClassName("logo"));
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);




            action.Build().Perform();
        }
        public void FirstContentNFinished(IWebDriver webDriver)
        {
            Wait(webDriver, By.ClassName("ltr-1axxs14"));
            webDriver.FindElement(By.ClassName("ltr-1axxs14"));
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.Tab);



            action.Build().Perform();
        }
        private void FirstContentCancel(IWebDriver webDriver)
        {
            Wait(webDriver, By.ClassName("search"));
            webDriver.FindElement(By.ClassName("search"));
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);


            action.Build().Perform();
        }
        private void LeftContent(IWebDriver webDriver)
        {
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.KeyDown(Keys.Shift);
            action.SendKeys(Keys.Tab);
            action.KeyUp(Keys.Shift);
            action.Build().Perform();
        }
        private void RightContent(IWebDriver webDriver)
        {
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.Tab);
            action.Build().Perform();
        }
        private void DownContent(IWebDriver webDriver)
        {
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.Build().Perform();
        }
        private void DownContentNFinished(IWebDriver webDriver)
        {
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);

            action.Build().Perform();
        }
        private void UpContent(IWebDriver webDriver)
        {
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.KeyDown(Keys.Shift);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.KeyUp(Keys.Shift);
            action.Build().Perform();
        }
        private void UpContentNFinished(IWebDriver webDriver)
        {
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.KeyDown(Keys.Shift);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);
            action.SendKeys(Keys.Tab);

            action.KeyUp(Keys.Shift);
            action.Build().Perform();
        }
        private void Wait(IWebDriver webDriver, By by)
        {
            try
            {
                WebDriverWait w = new WebDriverWait(webDriver, TimeSpan.FromSeconds(20));
                w.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(by));
                System.Threading.Thread.Sleep(500);
            }
            catch
            {
                //TO DO
                // Quando elemento nao encontrado retornar informacao ao utilizador que não possível e para função onde wait foi chamado
                Console.WriteLine("Not found");
            }

        }
        private void GoBackPage(IWebDriver webDriver)
        {
            webDriver.Navigate().Refresh();
            Actions action = new Actions(webDriver);

            action.SendKeys(Keys.Alt);
            action.SendKeys(Keys.ArrowLeft);

            action.Build().Perform();
        }
        private void RefreshPage(IWebDriver webDriver)
        {

            webDriver.Navigate().Refresh();
        }
        private void FecharCont(IWebDriver webDriver)
        {
            Wait(webDriver, By.ClassName("focus-trap-wrapper"));
            IWebElement div = webDriver.FindElement(By.ClassName("focus-trap-wrapper"));

            div.FindElement(By.ClassName("previewModal-close")).Click();
        }
        private void SairCont_reproduzir(IWebDriver webDriver)
        {
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            Wait(webDriver, By.ClassName("ltr-1212o1j"));
            action.MoveToElement(webDriver.FindElement(By.ClassName("ltr-1212o1j"))).Perform();
            Wait(webDriver, By.ClassName("ltr-14ph5iy"));
            webDriver.FindElement(By.ClassName("ltr-14ph5iy")).Click();
        }
        private void Play(IWebDriver webDriver)
        {

            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.Space);

            action.Build().Perform();
        }
        private void Saltar10(IWebDriver webDriver)
        {
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.ArrowRight);
            action.Build().Perform();
        }
        private void Retroceder10(IWebDriver webDriver)
        {
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.ArrowLeft);
            action.Build().Perform();
        }
        private void IgnorarIntro(IWebDriver webDriver)
        {
            Actions action = new Actions(webDriver);
            Wait(webDriver, By.ClassName("ltr-1212o1j"));
            action.MoveToElement(webDriver.FindElement(By.ClassName("ltr-1212o1j"))).Perform();
            Wait(webDriver, By.ClassName("ltr-1d02up3"));
            webDriver.FindElement(By.ClassName("ltr-1d02up3")).Click();
        }
        private void Pause(IWebDriver webDriver)
        {

            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.Space);

            action.Build().Perform();
        }
        private void Mutar(IWebDriver webDriver)
        {
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.ArrowDown);
            action.SendKeys(Keys.ArrowDown);
            action.SendKeys(Keys.ArrowDown);
            action.SendKeys(Keys.ArrowDown);
            action.SendKeys(Keys.ArrowDown);
            action.SendKeys(Keys.ArrowDown);
            action.SendKeys(Keys.ArrowDown);
            action.SendKeys(Keys.ArrowDown);
            action.SendKeys(Keys.ArrowDown);
            action.SendKeys(Keys.ArrowDown);
            action.SendKeys(Keys.ArrowDown);
            action.Build().Perform();
        }
        private void Desmutar(IWebDriver webDriver)
        {
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.ArrowUp);
            action.SendKeys(Keys.ArrowUp);
            action.SendKeys(Keys.ArrowUp);
            action.SendKeys(Keys.ArrowUp);
            action.SendKeys(Keys.ArrowUp);
            action.SendKeys(Keys.ArrowUp);
            action.SendKeys(Keys.ArrowUp);
            action.SendKeys(Keys.ArrowUp);
            action.SendKeys(Keys.ArrowUp);
            action.SendKeys(Keys.ArrowUp);
            action.SendKeys(Keys.ArrowUp);
            action.Build().Perform();
        }
        private void AumentarSom(IWebDriver webDriver)
        {
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.ArrowUp);
            action.SendKeys(Keys.ArrowUp);
            action.SendKeys(Keys.ArrowUp);
            action.SendKeys(Keys.ArrowUp);
            action.SendKeys(Keys.ArrowUp);

            action.Build().Perform();
        }
        private void DiminuirSom(IWebDriver webDriver)
        {
            System.Threading.Thread.Sleep(1000);
            Actions action = new Actions(webDriver);
            action.SendKeys(Keys.ArrowDown);
            action.SendKeys(Keys.ArrowDown);
            action.SendKeys(Keys.ArrowDown);
            action.SendKeys(Keys.ArrowDown);
            action.SendKeys(Keys.ArrowDown);

            action.Build().Perform();
        }
        private void Login(IWebDriver webDriver, String mail, String pass, String code)
        {

            Wait(webDriver, By.Name("userLoginId"));
            IWebElement login_mail = webDriver.FindElement(By.Name("userLoginId"));
            login_mail.SendKeys(mail);

            Wait(webDriver, By.Name("password"));
            IWebElement login_pass = webDriver.FindElement(By.Name("password"));
            login_pass.SendKeys(pass);
            System.Threading.Thread.Sleep(1000);

            Wait(webDriver, By.XPath("/html/body/div[1]/div/div[3]/div/div/div[1]/form/button"));
            webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[3]/div/div/div[1]/form/button")).Click();
            Wait(webDriver, By.XPath("/html/body/div[1]/div/div/div[1]/div[1]/div[2]/div/div/ul/li[1]/div/a/div/div"));
            webDriver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div[1]/div[2]/div/div/ul/li[1]/div/a/div/div")).Click();

            foreach (char c in code)
            {

                webDriver.SendKeys(By.ClassName("focus-visible"), c.ToString());
            }

        }
        private void ShowTopFilmes(IWebDriver webDriver)
        {
            webDriver.Navigate().GoToUrl("https://www.netflix.com/browse/genre/34399");
            IJavaScriptExecutor jse = (IJavaScriptExecutor)webDriver;
            Wait(webDriver, By.ClassName("forward-leaning"));
            IWebElement elementTop = webDriver.FindElement(By.ClassName("forward-leaning"));
            jse.ExecuteScript("arguments[0].scrollIntoView(true)", elementTop);
        }
        private void ShowNotFinished(IWebDriver webDriver)
        {
            webDriver.Navigate().GoToUrl("https://www.netflix.com/browse/m/continue-watching");
        }
        private void ShowTopSeries(IWebDriver webDriver)
        {
            webDriver.Navigate().GoToUrl("https://www.netflix.com/browse/genre/83");
            IJavaScriptExecutor jse = (IJavaScriptExecutor)webDriver;
            Wait(webDriver, By.ClassName("forward-leaning"));
            IWebElement elementTop = webDriver.FindElement(By.ClassName("forward-leaning"));
            jse.ExecuteScript("arguments[0].scrollIntoView(true)", elementTop);
        }

        private void SelectTopSeries(IWebDriver webDriver, String pos)
        {
            int posi = int.Parse(pos) - 1;
            if ((posi > 5 && AppContext.nextPage == false) || (posi <= 5 && AppContext.nextPage == true))
            {
                Wait(webDriver, By.ClassName("handleNext"));
                System.Threading.Thread.Sleep(1000);
                webDriver.FindElement(By.ClassName("handleNext")).Click();
            }
            System.Threading.Thread.Sleep(2000);
            Wait(webDriver, By.Id("title-card-1-" + posi));
            webDriver.FindElement(By.Id("title-card-1-" + posi)).Click();
        }
        private void NextPageFilmes(IWebDriver webDriver)
        {
            Wait(webDriver, By.ClassName("handleNext"));
            System.Threading.Thread.Sleep(1000);
            webDriver.FindElement(By.ClassName("handleNext")).Click();
        }
        private void NextPageSeries(IWebDriver webDriver)
        {
            Wait(webDriver, By.Id("row-4"));
            IWebElement div = webDriver.FindElement(By.Id("row-4"));
            div.FindElement(By.ClassName("handleNext")).Click();
        }
        private void SelectTop(IWebDriver webDriver, String pos)
        {
            int posi = int.Parse(pos) - 1;
            if ((posi > 5 && AppContext.nextPage == false) || (posi <= 5 && AppContext.nextPage == true))
            {
                Wait(webDriver, By.ClassName("handleNext"));
                System.Threading.Thread.Sleep(1000);
                webDriver.FindElement(By.ClassName("handleNext")).Click();
            }
            System.Threading.Thread.Sleep(2000);
            Wait(webDriver, By.Id("title-card-1-" + posi));
            webDriver.FindElement(By.Id("title-card-1-" + posi)).Click();
        }
        private void SelectContIdioma(IWebDriver webDriver, String linha, String pos)
        {
            int posi = int.Parse(pos) - 1;
            int linhai = int.Parse(linha) - 1;
            pos = posi.ToString();
            linha = linhai.ToString();
            System.Threading.Thread.Sleep(2000);
            Wait(webDriver, By.Id("title-card-" + linha + "-" + pos));
            webDriver.FindElement(By.Id("title-card-" + linha + "-" + pos)).Click();
        }
        private void SelectNotFinished(IWebDriver webDriver, String linha, String pos)
        {
            int posi = int.Parse(pos) - 1;
            int linhai = int.Parse(linha) - 1;
            pos = posi.ToString();
            linha = linhai.ToString();
            Wait(webDriver, By.ClassName("focus-trap-wrapper"));
            IWebElement div = webDriver.FindElement(By.ClassName("focus-trap-wrapper"));

            div.FindElement(By.Id("title-card-" + linha + "-" + pos)).Click();

        }
        private void SelecCont(IWebDriver webDriver, String linha, String cont)
        {
            Wait(webDriver, By.XPath("/html/body/div[1]/div/div/div[1]/div[1]/div[2]/div/div/div[2]/div[2]/div/div/div[" + linha + "]/div/div/div/div/div/div[" + cont + "]/div/div"));
            webDriver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div[1]/div[2]/div/div/div[2]/div[2]/div/div/div[" + linha + "]/div/div/div/div/div/div[" + cont + "]/div/div")).Click();
        }
        private void ReproduzirCont(IWebDriver webDriver)
        {
            System.Threading.Thread.Sleep(1000);
            Wait(webDriver, By.ClassName("focus-trap-wrapper"));
            IWebElement div = webDriver.FindElement(By.ClassName("focus-trap-wrapper"));

            div.FindElement(By.ClassName("ltr-1jtux27")).Click();
        }
        private void Pesquisar(IWebDriver webDriver, String text)
        {
            Wait(webDriver, By.ClassName("searchBox"));
            webDriver.FindElement(By.ClassName("searchBox")).Click();
            Wait(webDriver, By.Id("searchInput"));
            IWebElement pesq = webDriver.FindElement(By.Id("searchInput"));
            pesq.SendKeys(text);

        }
        private void ShowIdioma(IWebDriver webDriver, String code)
        {
            webDriver.Navigate().GoToUrl("https://www.netflix.com/browse/original-audio/" + code);
        }
        private void searchTypeOfFilm(string json)
        {

            switch (json)
            {
                case "ACTION":
                    Console.WriteLine("SELENIUM PESQUISAR FILMES DE AÇÃO");
                    WebDriver.webDriver.Navigate().GoToUrl("https://www.netflix.com/search?q=" + "10673");
                    FirstContent(WebDriver.webDriver);

                    synthesizer.Speak("Estes são os conteúdos de Ação disponíveis.");
                    AppContext.context = "SEARCH";
                    break;
                case "COMEDY":
                    Console.WriteLine("SELENIUM PESQUISAR FILMES DE COMÉDIA");
                    WebDriver.webDriver.Navigate().GoToUrl("https://www.netflix.com/search?q=" + "10375");
                    FirstContent(WebDriver.webDriver);
                    synthesizer.Speak("Estes são os conteúdos de Comédia disponíveis.");
                    AppContext.context = "SEARCH";
                    break;
                case "DRAMA":
                    Console.WriteLine("SELENIUM PESQUISAR FILMES DE DRAMA");
                    WebDriver.webDriver.Navigate().GoToUrl("https://www.netflix.com/search?q=" + "11714");
                    FirstContent(WebDriver.webDriver);
                    synthesizer.Speak("Estes são os conteúdos de Drama disponíveis.");
                    AppContext.context = "SEARCH";
                    break;
                case "NOVEL":
                    Console.WriteLine("SELENIUM PESQUISAR FILMES DE ROMANCE");
                    WebDriver.webDriver.Navigate().GoToUrl("https://www.netflix.com/search?q=" + "26156");
                    FirstContent(WebDriver.webDriver);

                    synthesizer.Speak("Estes são os conteúdos de Romance disponíveis.");
                    AppContext.context = "SEARCH";
                    break;
                case "TERROR":
                    Console.WriteLine("SELENIUM PESQUISAR FILMES DE TERROR");
                    WebDriver.webDriver.Navigate().GoToUrl("https://www.netflix.com/search?q=" + "83059");
                    FirstContent(WebDriver.webDriver);
                    synthesizer.Speak("Estes são os conteúdos de Terror disponíveis.");
                    AppContext.context = "SEARCH";
                    break;
                case "NATAL":
                    Console.WriteLine("SELENIUM PESQUISAR FILMES DE TERROR");
                    WebDriver.webDriver.Navigate().GoToUrl("https://www.netflix.com/search?q=" + "81346420");
                    FirstContent(WebDriver.webDriver);
                    synthesizer.Speak("Estes são os conteúdos de Natal disponíveis.");
                    AppContext.context = "SEARCH";
                    break;
                case "PT":
                    ShowIdioma(WebDriver.webDriver, "107465");
                    FirstContentIdioma(WebDriver.webDriver);
                    synthesizer.Speak("Estes são os conteúdos em Português disponíveis.");
                    AppContext.context = "IDIOMA";
                    break;
                case "FR":
                    ShowIdioma(WebDriver.webDriver, "100378");
                    FirstContentIdioma(WebDriver.webDriver);
                    synthesizer.Speak("Estes são os conteúdos em Francês disponíveis.");
                    AppContext.context = "IDIOMA";
                    break;
                case "ES":
                    ShowIdioma(WebDriver.webDriver, "100396");
                    FirstContentIdioma(WebDriver.webDriver);
                    synthesizer.Speak("Estes são os conteúdos em Espanhol disponíveis.");
                    AppContext.context = "IDIOMA";
                    break;
                case "EN":
                    ShowIdioma(WebDriver.webDriver, "107548");
                    FirstContentIdioma(WebDriver.webDriver);
                    synthesizer.Speak("Estes são os conteúdos em Inglês disponíveis.");
                    AppContext.context = "IDIOMA";
                    break;
            }
        }

        private void searchTypeOfSeries(string json)
        {
            switch (json)
            {
                case "ACTION":
                    Console.WriteLine("SELENIUM PESQUISAR FILMES DE AÇÃO");
                    Pesquisar(WebDriver.webDriver, "10673");
                    break;
                case "COMEDY":
                    Console.WriteLine("SELENIUM PESQUISAR FILMES DE COMÉDIA");
                    Pesquisar(WebDriver.webDriver, "10375");
                    break;
                case "DRAMA":
                    Console.WriteLine("SELENIUM PESQUISAR FILMES DE DRAMA");
                    Pesquisar(WebDriver.webDriver, "11714");
                    break;
                case "NOVEL":
                    Console.WriteLine("SELENIUM PESQUISAR FILMES DE ROMANCE");
                    Pesquisar(WebDriver.webDriver, "26156");
                    break;
                case "TERROR":
                    Console.WriteLine("SELENIUM PESQUISAR FILMES DE TERROR");
                    Pesquisar(WebDriver.webDriver, "83059");
                    break;
                case "CHRISTMAS":
                    Console.WriteLine("SELENIUM PESQUISAR FILMES DE TERROR");
                    Pesquisar(WebDriver.webDriver, "81346420");
                    break;
                case "PT":
                    ShowIdioma(WebDriver.webDriver, "107465");
                    AppContext.context = "IDIOMA";
                    break;
                case "FR":
                    ShowIdioma(WebDriver.webDriver, "100378");
                    AppContext.context = "IDIOMA";
                    break;
                case "ES":
                    ShowIdioma(WebDriver.webDriver, "100396");
                    AppContext.context = "IDIOMA";
                    break;
                case "EN":
                    ShowIdioma(WebDriver.webDriver, "107548");
                    AppContext.context = "IDIOMA";
                    break;
            }
        }
    }
}