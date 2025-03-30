class Program
{
    static readonly List<reporte> reportes = [];
    static string connectionString;
    static void Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Configuration.AddJsonFile("appsettings.json");
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        List<string> medicamentos = obtener_medicamentos();
        foreach (var medicamento in medicamentos)
        {
            Console.WriteLine($"Buscando información de {medicamento}...");
            scrape_data(medicamento);

            var reportes_unicos = reportes.Distinct().ToList();
            reportes.Clear();
            reportes.AddRange(reportes_unicos);

            export_to_database(reportes, medicamento);
        }

        Console.WriteLine("Proceso finalizado. Cerrando contenedor.");
    }

    static void scrape_data(string busqueda)
    {
        scrape_colsubsidio(busqueda);
        scrape_drogas_la_rebaja(busqueda);
        scrape_todo_drogas(busqueda);
        scrape_cruz_verde(busqueda);
    }

    static void scrape_colsubsidio(string busqueda)
    {
        try
        {
            ChromeOptions options = new();
            options.AddArgument("--start-maximized");
            options.AddArgument("--headless=new");  // Usa 'new' en lugar de solo '--headless'
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            // Directorio único para evitar conflictos
            string userDataDir = $"/tmp/chrome-{Guid.NewGuid()}";
            options.AddArgument($"--user-data-dir={userDataDir}");

            // Deshabilitar restauración de sesión y errores emergentes
            options.AddArgument("--disable-session-crashed-bubble");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-features=InfiniteSessionRestore");

            // Usar un puerto fijo en lugar de '0'
            options.AddArgument("--remote-debugging-port=9222");

            using IWebDriver driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl($"https://www.drogueriascolsubsidio.com/{busqueda}");
            Console.WriteLine($"Buscando en {nameof(scrape_colsubsidio)}...");

            WebDriverWait wait = new(driver, TimeSpan.FromSeconds(20));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
            wait.Until(d => d.FindElement(By.ClassName("dataproducto-nameProduct")).Displayed);

            IList<IWebElement> productos = driver.FindElements(By.ClassName("dataproducto-nameProduct"));

            for (int i = 0; i < productos.Count; i++)
            {
                try
                {
                    Thread.Sleep(5000);
                    productos[i].Click();
                    wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                    wait.Until(d => d.FindElement(By.CssSelector("h2.text-title.productName")).Displayed);
                    Thread.Sleep(5000);

                    string nombre = extraer_texto(driver, By.CssSelector("h2.text-title.productName"));
                    string laboratorio = extraer_texto(driver, By.CssSelector("p.ContentLaboratorio > div[data-atributename='Laboratorio']"));
                    string precio = extraer_texto(driver, By.CssSelector("div.bestPrice > p.js-bestPrice"));
                    string presentacion = extraer_texto(driver, By.CssSelector("p.ContentPresentaticones > div[data-atributename='Presentación']"));
                    string concentracion = extraer_texto(driver, By.CssSelector("p.ContentConcentracion > div[data-atributename='Concentración']"));
                    string principio_activo = extraer_texto(driver, By.CssSelector("p.ContentPrincipioActivo > div[data-atributename='Principio Activo']"));
                    string registro_invima = extraer_texto(driver, By.CssSelector("p.ContentInvima > div[data-atributename='ID Invima']"));
                    string fecha_extraccion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    reportes.Add(new()
                    {
                        farmacia = "Colsubsidio",
                        nombre = nombre,
                        laboratorio = laboratorio,
                        precio = precio,
                        presentacion = presentacion,
                        concentracion = concentracion,
                        principio_activo = principio_activo,
                        registro_invima = registro_invima,
                        fecha_extraccion = fecha_extraccion
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error al procesar el producto {i + 1}: {e.Message}. Continuando con el siguiente producto.");
                    driver.Navigate().Back();
                    wait.Until(d => (string)((IJavaScriptExecutor)d).ExecuteScript("return document.readyState") == "complete");
                    productos = driver.FindElements(By.ClassName("dataproducto-nameProduct"));
                    continue;
                }

                driver.Navigate().Back();
                wait.Until(d => (string)((IJavaScriptExecutor)d).ExecuteScript("return document.readyState") == "complete");
                productos = driver.FindElements(By.ClassName("dataproducto-nameProduct"));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al procesar la busqueda en {nameof(scrape_colsubsidio)}: {e.Message}.");
        }
    }

    static void scrape_drogas_la_rebaja(string busqueda)
    {
        try
        {
            ChromeOptions options = new();
            options.AddArgument("--start-maximized");
            options.AddArgument("--headless=new");  // Usa 'new' en lugar de solo '--headless'
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            // Directorio único para evitar conflictos
            string userDataDir = $"/tmp/chrome-{Guid.NewGuid()}";
            options.AddArgument($"--user-data-dir={userDataDir}");

            // Deshabilitar restauración de sesión y errores emergentes
            options.AddArgument("--disable-session-crashed-bubble");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-features=InfiniteSessionRestore");

            // Usar un puerto fijo en lugar de '0'
            options.AddArgument("--remote-debugging-port=9222");

            using IWebDriver driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl($"https://www.larebajavirtual.com/{busqueda}");
            Console.WriteLine($"Buscando en {nameof(scrape_drogas_la_rebaja)}...");

            WebDriverWait wait = new(driver, TimeSpan.FromSeconds(20));
            wait.Until(d => (string)((IJavaScriptExecutor)d).ExecuteScript("return document.readyState") == "complete");
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");

            wait.Until(d => d.FindElement(By.XPath("//a[contains(@class, 'vtex-product-summary-2-x-clearLink') and contains(@class, 'flex') and contains(@class, 'flex-column')]")).Displayed);

            IList<IWebElement> productos = driver.FindElements(By.XPath("//a[contains(@class, 'vtex-product-summary-2-x-clearLink') and contains(@class, 'flex') and contains(@class, 'flex-column')]"));

            for (int i = 0; i < productos.Count; i++)
            {
                try
                {
                    Thread.Sleep(5000);
                    var href = productos[i].GetAttribute("href");
                    Thread.Sleep(5000);
                    driver.Navigate().GoToUrl(href);

                    wait.Until(d => (string)((IJavaScriptExecutor)d).ExecuteScript("return document.readyState") == "complete");
                    Thread.Sleep(5000);

                    string nombre = extraer_texto(driver, By.XPath("//span[@class='vtex-store-components-3-x-productBrand ']"));
                    string laboratorio = extraer_texto(driver, By.XPath("//span[@class='vtex-store-components-3-x-productBrandName']"));
                    string precio = extraer_texto(driver, By.XPath("//span[@class='copservir-larebaja-theme-0-x-productPriceValue']"));
                    string presentacion = extraer_texto(driver, By.XPath("//div[@class='copservir-larebaja-theme-0-x-PUM-content-wrapper']/span"));
                    string concentracion = extraer_texto(driver, By.CssSelector("p.ContentConcentracion > div[data-atributename='Concentración']"));
                    string principio_activo = extraer_texto(driver, By.XPath("//div[@class='copservir-larebaja-theme-0-x-active-ingredient-container']//p[@class='copservir-larebaja-theme-0-x-active-ingredient-text']"));
                    string registro_invima = extraer_texto(driver, By.XPath("//div[@class='copservir-larebaja-theme-0-x-sanitary-register-container']//p[@class='copservir-larebaja-theme-0-x-sanitary-register-text']"));
                    string fecha_extraccion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    reportes.Add(new()
                    {
                        farmacia = "Drogas La Rebaja",
                        nombre = nombre,
                        laboratorio = laboratorio,
                        precio = precio,
                        presentacion = presentacion,
                        concentracion = concentracion,
                        principio_activo = principio_activo,
                        registro_invima = registro_invima,
                        fecha_extraccion = fecha_extraccion
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error al procesar el producto {i + 1}: {e.Message}. Continuando con el siguiente producto.");
                    driver.Navigate().Back();
                    wait.Until(d => (string)((IJavaScriptExecutor)d).ExecuteScript("return document.readyState") == "complete");
                    productos = driver.FindElements(By.XPath("//a[contains(@class, 'vtex-product-summary-2-x-clearLink') and contains(@class, 'flex') and contains(@class, 'flex-column')]"));
                    continue;
                }

                driver.Navigate().Back();
                wait.Until(d => (string)((IJavaScriptExecutor)d).ExecuteScript("return document.readyState") == "complete");
                productos = driver.FindElements(By.XPath("//a[contains(@class, 'vtex-product-summary-2-x-clearLink') and contains(@class, 'flex') and contains(@class, 'flex-column')]"));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al procesar la busqueda en {nameof(scrape_drogas_la_rebaja)}: {e.Message}.");
        }
    }

    static void scrape_todo_drogas(string busqueda)
    {
        try
        {
            ChromeOptions options = new();
            options.AddArgument("--start-maximized");
            options.AddArgument("--headless=new");  // Usa 'new' en lugar de solo '--headless'
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            // Directorio único para evitar conflictos
            string userDataDir = $"/tmp/chrome-{Guid.NewGuid()}";
            options.AddArgument($"--user-data-dir={userDataDir}");

            // Deshabilitar restauración de sesión y errores emergentes
            options.AddArgument("--disable-session-crashed-bubble");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-features=InfiniteSessionRestore");

            // Usar un puerto fijo en lugar de '0'
            options.AddArgument("--remote-debugging-port=9222");

            using IWebDriver driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl($"https://tododrogas.com.co/inicio/index.php?id=0&search={busqueda}");
            Console.WriteLine($"Buscando en {nameof(scrape_todo_drogas)}...");

            WebDriverWait wait = new(driver, TimeSpan.FromSeconds(20));
            wait.Until(d => d.FindElement(By.XPath("//a[contains(@href, 'contenido-detalle.php')]")).Displayed);

            IList<IWebElement> productos = driver.FindElements(By.XPath("//a[contains(@href, 'contenido-detalle.php')]"));

            for (int i = 0; i < productos.Count; i++)
            {
                try
                {
                    Thread.Sleep(5000);

                    string href = productos[i].GetAttribute("href");
                    driver.Navigate().GoToUrl(href);

                    wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                    Thread.Sleep(5000);

                    string nombre = extraer_texto(driver, By.XPath("//div[@class='col-md-6 col-xs-12 agileinfo_single_right']/h2"));
                    string textoCompleto = driver.FindElement(By.XPath("//div[@class='rating1']")).GetAttribute("innerText");
                    string laboratorio = extraer_marca(textoCompleto);
                    string registro_invima = extraer_registro_invima(textoCompleto);
                    string precio = extraer_texto(driver, By.CssSelector(".col-md-6.col-xs-12.agileinfo_single_right .m-sing"));
                    string presentacion = extraer_texto(driver, By.XPath("//div[@class='copservir-larebaja-theme-0-x-PUM-content-wrapper']/span"));
                    string concentracion = extraer_texto(driver, By.CssSelector("p.ContentConcentracion > div[data-atributename='Concentración']"));
                    string principio_activo = extraer_texto(driver, By.XPath("//div[@class='copservir-larebaja-theme-0-x-active-ingredient-container']//p[@class='copservir-larebaja-theme-0-x-active-ingredient-text']"));
                    string fecha_extraccion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    reportes.Add(new()
                    {
                        farmacia = "Todo Drogas",
                        nombre = nombre,
                        laboratorio = laboratorio,
                        precio = precio,
                        presentacion = presentacion,
                        concentracion = concentracion,
                        principio_activo = principio_activo,
                        registro_invima = registro_invima,
                        fecha_extraccion = fecha_extraccion
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error al procesar el producto {i + 1}: {e.Message}. Continuando con el siguiente producto.");
                    driver.Navigate().Back();
                    wait.Until(d => (string)((IJavaScriptExecutor)d).ExecuteScript("return document.readyState") == "complete");
                    productos = driver.FindElements(By.XPath("//a[contains(@class, 'vtex-product-summary-2-x-clearLink') and contains(@class, 'flex') and contains(@class, 'flex-column')]"));
                    continue;
                }

                driver.Navigate().Back();
                wait.Until(d => (string)((IJavaScriptExecutor)d).ExecuteScript("return document.readyState") == "complete");
                productos = driver.FindElements(By.XPath("//a[contains(@class, 'vtex-product-summary-2-x-clearLink') and contains(@class, 'flex') and contains(@class, 'flex-column')]"));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al procesar la busqueda en {nameof(scrape_todo_drogas)}: {e.Message}.");
        }
    }

    static void scrape_cruz_verde(string busqueda)
    {
        try
        {
            ChromeOptions options = new();
            options.AddArgument("--start-maximized");
            options.AddArgument("--headless=new");  // Usa 'new' en lugar de solo '--headless'
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            // Directorio único para evitar conflictos
            string userDataDir = $"/tmp/chrome-{Guid.NewGuid()}";
            options.AddArgument($"--user-data-dir={userDataDir}");

            // Deshabilitar restauración de sesión y errores emergentes
            options.AddArgument("--disable-session-crashed-bubble");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-features=InfiniteSessionRestore");

            // Usar un puerto fijo en lugar de '0'
            options.AddArgument("--remote-debugging-port=9222");
            options.AddArgument("--guest");

            using IWebDriver driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl($"https://www.cruzverde.com.co/search?query={busqueda}");
            Console.WriteLine($"Buscando en {nameof(scrape_cruz_verde)}...");

            WebDriverWait wait = new(driver, TimeSpan.FromSeconds(20));
            wait.Until(d => (string)((IJavaScriptExecutor)d).ExecuteScript("return document.readyState") == "complete");
            wait.Until(d => d.FindElement(By.XPath("//at-image[contains(@class, 'max-w-sm') and contains(@class, 'h-full') and contains(@class, 'w-4/5')]//a[@_ngcontent-serverapp-c37]")).Displayed);

            IList<IWebElement> productos = driver.FindElements(By.XPath("//at-image[contains(@class, 'max-w-sm') and contains(@class, 'h-full') and contains(@class, 'w-4/5')]//a[@_ngcontent-serverapp-c37]"));

            for (int i = 0; i < productos.Count; i++)
            {
                try
                {
                    Thread.Sleep(5000);
                    string href = productos[i].GetAttribute("href");
                    driver.Navigate().GoToUrl(href);

                    wait.Until(d => (string)((IJavaScriptExecutor)d).ExecuteScript("return document.readyState") == "complete");

                    string nombre = extraer_texto(driver, By.XPath("//h1[@class='text-28 leading-35 font-bold w-3/4']"));
                    string laboratorio = extraer_texto(driver, By.XPath("//div[@class='text-16 uppercase italic cursor-pointer hover:text-accent']"));
                    string precio = extraer_texto(driver, By.XPath("//span[@class='font-bold text-prices']"));
                    string principio_activo = extraer_texto_con_js(driver, By.XPath("/html/body/app-root/app-page/.../ml-rich-text/div/p[2]"));
                    string presentacion = extraer_texto_con_js(driver, By.XPath("/html/body/app-root/app-page/.../ml-rich-text/div/p[4]"));
                    string concentracion = extraer_texto_con_js(driver, By.XPath("/html/body/app-root/app-page/.../ml-rich-text/div/p[3]"));
                    string registro_invima = extraer_texto(driver, By.XPath("//span[@class='text-12 text-gray-dark ng-star-inserted']"));
                    string fecha_extraccion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    reportes.Add(new()
                    {
                        farmacia = "Cruz Verde",
                        nombre = nombre,
                        laboratorio = laboratorio,
                        precio = precio,
                        presentacion = presentacion,
                        concentracion = concentracion,
                        principio_activo = principio_activo,
                        registro_invima = registro_invima,
                        fecha_extraccion = fecha_extraccion
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error al procesar el producto {i + 1}: {e.Message}. Continuando con el siguiente producto.");
                    driver.Navigate().Back();
                    wait.Until(d => (string)((IJavaScriptExecutor)d).ExecuteScript("return document.readyState") == "complete");
                    productos = driver.FindElements(By.XPath("//at-image[contains(@class, 'max-w-sm') and contains(@class, 'h-full') and contains(@class, 'w-4/5')]//a[@_ngcontent-serverapp-c37]"));
                    continue;
                }

                driver.Navigate().Back();
                wait.Until(d => (string)((IJavaScriptExecutor)d).ExecuteScript("return document.readyState") == "complete");
                productos = driver.FindElements(By.XPath("//at-image[contains(@class, 'max-w-sm') and contains(@class, 'h-full') and contains(@class, 'w-4/5')]//a[@_ngcontent-serverapp-c37]"));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al procesar la busqueda en {nameof(scrape_cruz_verde)}: {e.Message}.");
        }
    }

    static string extraer_texto(IWebDriver driver, By selector)
    {
        try
        {
            return driver.FindElement(selector).Text;
        }
        catch (NoSuchElementException)
        {
            return "Sin información";
        }
    }

    static string extraer_marca(string texto)
    {
        string patron = @"Marca:\s*([^\n]+)";
        Match coincidencia = Regex.Match(texto, patron);
        return coincidencia.Success ? coincidencia.Groups[1].Value.Trim() : null;
    }

    static string extraer_registro_invima(string texto)
    {
        string patron = @"Registro INVIMA:\s*([^\n]+)";
        Match coincidencia = Regex.Match(texto, patron);
        return coincidencia.Success ? coincidencia.Groups[1].Value.Trim() : null;
    }

    static string extraer_texto_con_js(IWebDriver driver, By selector)
    {
        try
        {
            IWebElement elemento = driver.FindElement(selector);
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView(true);", elemento);
            string texto = (string)js.ExecuteScript("return arguments[0].textContent;", elemento);
            return !string.IsNullOrWhiteSpace(texto) ? texto.Trim() : "Sin información";
        }
        catch (Exception)
        {
            return "Sin información";
        }
    }

    static List<string> obtener_medicamentos()
    {
        List<string> medications = [];
        string query = "SELECT nombre FROM medicamento";

        using SqlConnection conn = new(connectionString);
        conn.Open();

        using SqlCommand cmd = new(query, conn);
        using SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            medications.Add(reader.GetString(0));
        }

        return medications;
    }

    static void export_to_database(List<reporte> reporte, string busqueda)
    {
        using SqlConnection conn = new(connectionString);
        conn.Open();

        using SqlTransaction transaction = conn.BeginTransaction();

        try
        {
            //using SqlCommand deleteCmd = new("DELETE FROM reporte WHERE medicamento = @medicamento", conn, transaction);
            //deleteCmd.Parameters.AddWithValue("@medicamento", busqueda);
            //deleteCmd.ExecuteNonQuery();

            foreach (var medicamento in reporte)
            {
                string query = "INSERT INTO reporte (farmacia, nombre, laboratorio, precio, presentacion, concentracion, principio_activo, registro_invima, fecha_extraccion, medicamento) " +
                               "VALUES (@farmacia, @nombre, @laboratorio, @precio, @presentacion, @concentracion, @principio_activo, @registro_invima, @fecha_extraccion, @medicamento)";

                using SqlCommand cmd = new(query, conn, transaction);
                cmd.Parameters.AddWithValue("@farmacia", medicamento.farmacia);
                cmd.Parameters.AddWithValue("@nombre", medicamento.nombre);
                cmd.Parameters.AddWithValue("@laboratorio", medicamento.laboratorio);
                cmd.Parameters.AddWithValue("@precio", medicamento.precio);
                cmd.Parameters.AddWithValue("@presentacion", medicamento.presentacion);
                cmd.Parameters.AddWithValue("@concentracion", medicamento.concentracion);
                cmd.Parameters.AddWithValue("@principio_activo", medicamento.principio_activo);
                cmd.Parameters.AddWithValue("@registro_invima", medicamento.registro_invima);
                cmd.Parameters.AddWithValue("@fecha_extraccion", medicamento.fecha_extraccion);
                cmd.Parameters.AddWithValue("@medicamento", busqueda);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    class reporte
    {
        public string farmacia { get; set; }
        public string nombre { get; set; }
        public string laboratorio { get; set; }
        public string precio { get; set; }
        public string presentacion { get; set; }
        public string concentracion { get; set; }
        public string principio_activo { get; set; }
        public string registro_invima { get; set; }
        public string fecha_extraccion { get; set; }
    }
}