using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using PdfSharp.Drawing;
using NavajaSuizaPDF.Logica;
using Tesseract;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Diagnostics;

namespace NavajaSuizaPDF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // ==========================================
        // MODO OSCURO
        // ==========================================
        private bool esModoOscuro = false;


// CONFIGURACIÓN DE TU REPOSITORIO (¡CAMBIA ESTO!)
        private const string GITHUB_USER = "gustavocazar3s-ship-it"; // Ej: JuanPerezDev
        private const string GITHUB_REPO = "NavajaSuizaPDF.";    // El nombre de tu repo
        private const string VERSION_ACTUAL = "3.0";            // Tu versión actual

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // GitHub exige un "User-Agent" para responder
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("NavajaSuizaApp", "1.0"));

                    // Consultamos la última versión (Release)
                    string urlApi = $"https://api.github.com/repos/{GITHUB_USER}/{GITHUB_REPO}/releases/latest";
                    var response = await client.GetStringAsync(urlApi);

                    // Buscamos la etiqueta "tag_name" en el JSON (manera rápida)
                    using (JsonDocument doc = JsonDocument.Parse(response))
                    {
                        string ultimaVersion = doc.RootElement.GetProperty("tag_name").GetString();
                        
                        // Limpiamos la 'v' si existe (ej: v3.1 -> 3.1)
                        string versionLimpia = ultimaVersion.Replace("v", "").Trim();

                        // Comparamos
                        if (EsVersionMayor(versionLimpia, VERSION_ACTUAL))
                        {
                            var resultado = MessageBox.Show(
                                $"¡Nueva versión disponible: {ultimaVersion}!\n\nTu versión: {VERSION_ACTUAL}\n\n¿Quieres descargarla ahora?", 
                                "Actualización Detectada", 
                                MessageBoxButton.YesNo, 
                                MessageBoxImage.Information);

                            if (resultado == MessageBoxResult.Yes)
                            {
                                // Abrir el navegador en la página de descargas
                                string urlDescarga = $"https://github.com/{GITHUB_USER}/{GITHUB_REPO}/releases/latest";
                                Process.Start(new ProcessStartInfo { FileName = urlDescarga, UseShellExecute = true });
                            }
                        }
                        else
                        {
                            MessageBox.Show("¡Ya tienes la última versión!", "Todo actualizado", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo buscar actualizaciones.\nRevisa tu internet o intenta más tarde.", "Error de Conexión");
            }
        }

        // Función auxiliar para comparar versiones (Ej: 3.1 es mayor que 3.0)
        private bool EsVersionMayor(string nueva, string actual)
        {
            try
            {
                var vNueva = new Version(nueva);
                var vActual = new Version(actual);
                return vNueva > vActual;
            }
            catch
            {
                // Si falla la conversión, comparamos texto simple
                return string.Compare(nueva, actual) > 0;
            }
        }

        // Función para abrir el enlace de donación
private void btnDonar_Click(object sender, RoutedEventArgs e)
{
    // CAMBIA ESTO POR TU LINK DE KO-FI REAL
    string urlDestino = "https://ko-fi.com/fibrici"; 

    try
    {
        // Truco para abrir navegador en .NET moderno
        Process.Start(new ProcessStartInfo
        {
            FileName = urlDestino,
            UseShellExecute = true
        });
    }
    catch (Exception)
    {
        MessageBox.Show("No se pudo abrir el enlace. Visita: " + urlDestino);
    }
}

        private void btnTema_Click(object sender, RoutedEventArgs e)
        {
            esModoOscuro = !esModoOscuro;
            if (esModoOscuro)
            {
                CambiarColor("BrushFondo", "#0F172A");
                CambiarColor("BrushTarjeta", "#1E293B");
                CambiarColor("BrushTextoTitulo", "#E2E8F0");
                CambiarColor("BrushTextoSub", "#94A3B8");
                CambiarColor("BrushMarcaFondo", "#1E3A8A"); // Azul oscuro nocturno
                CambiarColor("BrushMarcaTexto", "#60A5FA"); // Azul neón brillante
                if (sender is Button btn) btn.Content = new TextBlock { Text = "☀️", FontSize = 24 };
            }
            else
            {
                CambiarColor("BrushFondo", "#F3F4F6");
                CambiarColor("BrushTarjeta", "#FFFFFF");
                CambiarColor("BrushTextoTitulo", "#1E293B");
                CambiarColor("BrushTextoSub", "#64748B");
                CambiarColor("BrushMarcaFondo", "#EFF6FF");
                CambiarColor("BrushMarcaTexto", "#2563EB");
                if (sender is Button btn) btn.Content = new TextBlock { Text = "🌙", FontSize = 24 };
            }
        }

        private void CambiarColor(string claveRecurso, string hexColor)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                this.Resources[claveRecurso] = new SolidColorBrush(color);
            }
            catch { }
        }

        // ==========================================
        // CONTROL DE INTERFAZ
        // ==========================================
        private void ResetearInterfaz(bool trabajando)
        {
            if (trabajando)
            {
                pbProgreso.Visibility = Visibility.Visible;
                pbProgreso.Value = 0;
                btnMergePdf.IsEnabled = false;
                btnWordToPdf.IsEnabled = false;
                btnPdfToWord.IsEnabled = false;
                btnCompressPdf.IsEnabled = false;
                btnImgToPdf.IsEnabled = false;
                btnProtectPdf.IsEnabled = false;
                btnWatermark.IsEnabled = false;
                btnTema.IsEnabled = false;
                btnUnlockPdf.IsEnabled = false;
                btnOcr.IsEnabled = false;
                btnSplitPdf.IsEnabled = false;  
                btnRotatePdf.IsEnabled = false;  
                btnMetadata.IsEnabled = false;   
                btnTema.IsEnabled = false;
            }
            else
            {
                pbProgreso.Visibility = Visibility.Hidden;
                pbProgreso.Value = 0;
                btnMergePdf.IsEnabled = true;
                btnWordToPdf.IsEnabled = true;
                btnPdfToWord.IsEnabled = true;
                btnCompressPdf.IsEnabled = true;
                btnImgToPdf.IsEnabled = true;
                btnProtectPdf.IsEnabled = true;
                btnWatermark.IsEnabled = true;
                btnTema.IsEnabled = true;
                btnUnlockPdf.IsEnabled = true;
                btnOcr.IsEnabled = true;
                btnSplitPdf.IsEnabled = true;   
                btnRotatePdf.IsEnabled = true;  
                btnMetadata.IsEnabled = true;   
                btnTema.IsEnabled = true;
            }
        }

        // ==========================================
        // HERRAMIENTAS
        // ==========================================

        // 1. UNIR PDFS
        private async void btnMergePdf_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Multiselect = true, Filter = "PDF|*.pdf", Title = "Unir PDFs" };
            if (ofd.ShowDialog() == true)
            {
                SaveFileDialog sfd = new SaveFileDialog { Filter = "PDF|*.pdf", FileName = "Unido.pdf" };
                if (sfd.ShowDialog() == true)
                {
                    ResetearInterfaz(true);
                    txtStatus.Text = "Uniendo...";
                    await Task.Run(() =>
                    {
                        try
                        {
                            using (PdfDocument output = new PdfDocument())
                            {
                                int total = ofd.FileNames.Length;
                                for (int i = 0; i < total; i++)
                                {
                                    string file = ofd.FileNames[i];
                                    Dispatcher.Invoke(() => { 
                                        pbProgreso.Value = ((double)(i+1)/total)*100; 
                                        txtStatus.Text = $"Uniendo: {Path.GetFileName(file)}";
                                    });
                                    using (PdfDocument input = PdfReader.Open(file, PdfDocumentOpenMode.Import))
                                    {
                                        int count = input.PageCount;
                                        for (int idx = 0; idx < count; idx++) output.AddPage(input.Pages[idx]);
                                    }
                                }
                                output.Save(sfd.FileName);
                                Dispatcher.Invoke(() => MessageBox.Show("¡PDFs unidos con éxito!", "Listo"));
                            }
                        }
                        catch (Exception ex) { Dispatcher.Invoke(() => MessageBox.Show("Error: " + ex.Message)); }
                    });
                    ResetearInterfaz(false);
                    txtStatus.Text = "Listo.";
                }
            }
        }

        // 2. IMÁGENES A PDF
        private async void btnImgToPdf_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Multiselect = true, Filter = "Imágenes|*.jpg;*.png;*.jpeg", Title = "Seleccionar Imágenes" };
            if (ofd.ShowDialog() == true)
            {
                SaveFileDialog sfd = new SaveFileDialog { Filter = "PDF|*.pdf", FileName = "Album.pdf" };
                if (sfd.ShowDialog() == true)
                {
                    ResetearInterfaz(true);
                    await Task.Run(() =>
                    {
                        try
                        {
                            using (PdfDocument doc = new PdfDocument())
                            {
                                int total = ofd.FileNames.Length;
                                for (int i = 0; i < total; i++)
                                {
                                    string imgPath = ofd.FileNames[i];
                                    Dispatcher.Invoke(() => {
                                        pbProgreso.Value = ((double)(i + 1) / total) * 100;
                                        txtStatus.Text = $"Procesando: {Path.GetFileName(imgPath)}";
                                    });

                                    PdfPage page = doc.AddPage();
                                    using (XImage img = XImage.FromFile(imgPath))
                                    {
                                        using (XGraphics gfx = XGraphics.FromPdfPage(page))
                                        {
                                            double width = page.Width.Point;
                                            double height = (double)img.PixelHeight * width / (double)img.PixelWidth;
                                            if (height > page.Height.Point)
                                            {
                                                height = page.Height.Point;
                                                width = (double)img.PixelWidth * height / (double)img.PixelHeight;
                                            }
                                            gfx.DrawImage(img, 0, 0, width, height);
                                        }
                                    }
                                }
                                doc.Save(sfd.FileName);
                                Dispatcher.Invoke(() => MessageBox.Show("¡Imágenes convertidas a PDF!", "Éxito"));
                            }
                        }
                        catch (Exception ex) { Dispatcher.Invoke(() => MessageBox.Show("Error: " + ex.Message)); }
                    });
                    ResetearInterfaz(false);
                    txtStatus.Text = "Listo.";
                }
            }
        }

        // 3. PROTEGER PDF
        private async void btnProtectPdf_Click(object sender, RoutedEventArgs e)
        {
            DialogoPassword inputDialog = new DialogoPassword();
            if (inputDialog.ShowDialog() == true)
            {
                string password = inputDialog.Password;

                ProcesarArchivos("Proteger PDF", "PDF|*.pdf", "_Protegido.pdf", (origen, destino) =>
                {
                    using (PdfDocument doc = PdfReader.Open(origen, PdfDocumentOpenMode.Modify))
                    {
                        PdfSecuritySettings security = doc.SecuritySettings;
                        security.UserPassword = password;
                        security.OwnerPassword = password;
                        doc.Save(destino);
                    }
                });
            }
        }

        // 4. WORD A PDF
        private void btnWordToPdf_Click(object sender, RoutedEventArgs e)
        {
            ProcesarArchivos("Word a PDF", "Word|*.docx;*.doc", ".pdf", (origen, destino) => new GestorWord().ConvertirWordAPdf(origen, destino));
        }

        // 5. PDF A WORD
        private void btnPdfToWord_Click(object sender, RoutedEventArgs e)
        {
            ProcesarArchivos("PDF a Word", "PDF|*.pdf", ".docx", (origen, destino) => new GestorWord().ConvertirPdfAWord(origen, destino));
        }

        // 6. COMPRIMIR PDF
        private void btnCompressPdf_Click(object sender, RoutedEventArgs e)
        {
            ProcesarArchivos("Comprimir PDF", "PDF|*.pdf", "_Ligero.pdf", (origen, destino) => new GestorWord().ComprimirPdf(origen, destino));
        }

        // 7. MARCA DE AGUA
       private async void btnWatermark_Click(object sender, RoutedEventArgs e)
        {
            DialogoTexto inputDialog = new DialogoTexto("Texto de la marca:", "CONFIDENCIAL");
            if (inputDialog.ShowDialog() == true)
            {
                string marca = inputDialog.TextoIngresado;

                ProcesarArchivos("Marca de Agua", "PDF|*.pdf", "_Marca.pdf", (origen, destino) =>
                {
                    using (PdfDocument doc = PdfReader.Open(origen, PdfDocumentOpenMode.Modify))
                    {
                        foreach (PdfPage page in doc.Pages)
                        {
                            using (XGraphics gfx = XGraphics.FromPdfPage(page))
                            {
                                // 1. FUENTE: Usamos el constructor simple para evitar error de 'Bold'
                                XFont font = new XFont("Arial", 80); 
                                
                                // 2. MEDIR
                                XSize size = gfx.MeasureString(marca, font);

                                // 3. CENTRO DE PÁGINA (Aquí SÍ usamos .Point)
                                double centerX = page.Width.Point / 2;
                                double centerY = page.Height.Point / 2;

                                gfx.TranslateTransform(centerX, centerY);
                                gfx.RotateTransform(-45);

                                // 4. DIBUJAR (Aquí NO usamos .Point en 'size')
                                gfx.DrawString(marca, font, XBrushes.LightGray, 
                                    new XPoint(-size.Width / 2, -size.Height / 2), 
                                    XStringFormats.Default);
                            }
                        }
                        doc.Save(destino);
                    }
                });
            }
        }

        // 8. DESBLOQUEAR PDF// 8. DESBLOQUEAR PDF (Remove Security)
        private async void btnUnlockPdf_Click(object sender, RoutedEventArgs e)
        {
            ProcesarArchivos("Desbloquear PDF", "PDF|*.pdf", "_Desbloqueado.pdf", (origen, destino) =>
            {
                // Truco: Abrimos en modo "Import" crea una copia fresca sin metadatos de seguridad antiguos
                using (PdfDocument inputDoc = PdfReader.Open(origen, PdfDocumentOpenMode.Import))
                {
                    using (PdfDocument outputDoc = new PdfDocument())
                    {
                        // Copiamos página por página a un documento nuevo "virgen"
                        int totalPages = inputDoc.PageCount;
                        for (int i = 0; i < totalPages; i++)
                        {
                            outputDoc.AddPage(inputDoc.Pages[i]);
                        }
                        
                        // Guardamos sin ninguna configuración de seguridad
                        outputDoc.Save(destino);
                    }
                }
            });
        }

        // 9. OCR (IMAGEN A TEXTO)
        private async void btnOcr_Click(object sender, RoutedEventArgs e)
        {
            // TRUCO: Obtenemos la ruta real de donde está el .exe corriendo
            string rutaBase = AppDomain.CurrentDomain.BaseDirectory;
            string carpetaTessdata = System.IO.Path.Combine(rutaBase, "tessdata");

            // Verificamos usando la ruta absoluta
            if (!Directory.Exists(carpetaTessdata))
            {
                MessageBox.Show($"Error Crítico:\nNo encuentro la carpeta de datos en:\n{carpetaTessdata}\n\nAsegúrate de copiar la carpeta 'tessdata' junto al .exe.", "Faltan datos");
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog { Multiselect = true, Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp", Title = "Seleccionar imágenes para leer" };
            
            if (ofd.ShowDialog() == true)
            {
                SaveFileDialog sfd = new SaveFileDialog { Filter = "Archivo de Texto|*.txt", FileName = "TextoExtraido.txt" };
                if (sfd.ShowDialog() == true)
                {
                    ResetearInterfaz(true);
                    txtStatus.Text = "Leyendo texto..."; // El OCR puede tardar un poco

                    await Task.Run(() =>
                    {
                        try
                        {
                            // Inicializamos el motor en Español ("spa")
                            using (var engine = new TesseractEngine(carpetaTessdata, "spa", EngineMode.Default))
                            {
                                using (StreamWriter writer = new StreamWriter(sfd.FileName))
                                {
                                    int total = ofd.FileNames.Length;
                                    for (int i = 0; i < total; i++)
                                    {
                                        string imgPath = ofd.FileNames[i];
                                        
                                        Dispatcher.Invoke(() => {
                                            pbProgreso.Value = ((double)(i + 1) / total) * 100;
                                            txtStatus.Text = $"Leyendo: {Path.GetFileName(imgPath)}";
                                        });

                                        // Cargar imagen y procesar
                                        using (var img = Pix.LoadFromFile(imgPath))
                                        {
                                            using (var page = engine.Process(img))
                                            {
                                                string texto = page.GetText();
                                                
                                                // Escribir en el archivo final
                                                writer.WriteLine($"--- TEXTO DE: {Path.GetFileName(imgPath)} ---");
                                                writer.WriteLine(texto);
                                                writer.WriteLine("\n");
                                            }
                                        }
                                    }
                                }
                            }
                            Dispatcher.Invoke(() => MessageBox.Show("¡Lectura completada! Revisa el archivo de texto.", "Éxito"));
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(() => MessageBox.Show($"Error en OCR: {ex.Message}\n\n¿Descargaste 'spa.traineddata'?", "Error"));
                        }
                    });

                    ResetearInterfaz(false);
                    txtStatus.Text = "Listo.";
                }
            }
        }

        // 10. EXTRAER PÁGINAS (SPLIT)
        private async void btnSplitPdf_Click(object sender, RoutedEventArgs e)
        {
            DialogoTexto inputDialog = new DialogoTexto("Rango de páginas (ej. 1-3 o 5):", "1");
            if (inputDialog.ShowDialog() == true)
            {
                string rango = inputDialog.TextoIngresado;

                ProcesarArchivos("Extraer Páginas", "PDF|*.pdf", "_Extraido.pdf", (origen, destino) =>
                {
                    using (PdfDocument inputDoc = PdfReader.Open(origen, PdfDocumentOpenMode.Import))
                    {
                        using (PdfDocument outputDoc = new PdfDocument())
                        {
                            // Intentamos interpretar lo que escribió el usuario (ej. "1-5")
                            int inicio = 1;
                            int fin = 1;

                            if (rango.Contains("-"))
                            {
                                var partes = rango.Split('-');
                                int.TryParse(partes[0], out inicio);
                                int.TryParse(partes[1], out fin);
                            }
                            else
                            {
                                int.TryParse(rango, out inicio);
                                fin = inicio;
                            }

                            // Validar que no pida páginas que no existen
                            if (inicio < 1) inicio = 1;
                            if (fin > inputDoc.PageCount) fin = inputDoc.PageCount;

                            // Copiar las páginas (Restamos 1 porque el código cuenta desde 0)
                            for (int i = inicio - 1; i < fin; i++)
                            {
                                if (i >= 0 && i < inputDoc.PageCount)
                                {
                                    outputDoc.AddPage(inputDoc.Pages[i]);
                                }
                            }
                            outputDoc.Save(destino);
                        }
                    }
                });
            }
        }

        // 11. ROTAR PDF (90 GRADOS)
        private void btnRotatePdf_Click(object sender, RoutedEventArgs e)
        {
            ProcesarArchivos("Rotar PDF", "PDF|*.pdf", "_Rotado.pdf", (origen, destino) =>
            {
                using (PdfDocument doc = PdfReader.Open(origen, PdfDocumentOpenMode.Modify))
                {
                    foreach (PdfPage page in doc.Pages)
                    {
                        // Girar 90 grados a la derecha
                        page.Rotate = (page.Rotate + 90) % 360;
                    }
                    doc.Save(destino);
                }
            });
        }

        // 12. LIMPIAR METADATOS (MODO CIBERSEGURIDAD)
        private void btnMetadata_Click(object sender, RoutedEventArgs e)
        {
            ProcesarArchivos("Limpiar Metadatos", "PDF|*.pdf", "_Anonimo.pdf", (origen, destino) =>
            {
                using (PdfDocument doc = PdfReader.Open(origen, PdfDocumentOpenMode.Modify))
                {
                    // Borramos toda la información personal del archivo
                    doc.Info.Title = "";
                    doc.Info.Author = "";
                    doc.Info.Subject = "";
                    doc.Info.Keywords = "";
                    doc.Info.Creator = "";
                    doc.Info.Creator = "Navaja Suiza PDF";
                   // doc.Info.Producer = "Navaja Suiza PDF"; // Marca de la casa ;)
                    
                    doc.Save(destino);
                }
            });
        }

        // HELPER GENÉRICO
        private async void ProcesarArchivos(string titulo, string filtro, string extDestino, Action<string, string> accion)
        {
            OpenFileDialog ofd = new OpenFileDialog { Multiselect = true, Filter = filtro, Title = titulo };
            if (ofd.ShowDialog() == true)
            {
                ResetearInterfaz(true);
                int total = ofd.FileNames.Length;
                int exitos = 0;

                await Task.Run(() =>
                {
                    for (int i = 0; i < total; i++)
                    {
                        string origen = ofd.FileNames[i];
                        string destino;
                        
                        if (extDestino.StartsWith("_")) 
                            destino = Path.Combine(Path.GetDirectoryName(origen), Path.GetFileNameWithoutExtension(origen) + extDestino);
                        else 
                            destino = Path.Combine(Path.GetDirectoryName(origen), Path.GetFileNameWithoutExtension(origen) + extDestino);

                        try
                        {
                            Dispatcher.Invoke(() => {
                                pbProgreso.Value = ((double)(i + 1) / total) * 100;
                                txtStatus.Text = $"Procesando: {Path.GetFileName(origen)}";
                            });
                            
                            accion(origen, destino);
                            exitos++;
                        }
                        catch (Exception ex) 
                        { 
                            // <--- AQUÍ ESTÁ EL CAMBIO: Ahora mostramos el error
                            Dispatcher.Invoke(() => MessageBox.Show($"Error en '{Path.GetFileName(origen)}':\n{ex.Message}", "Ups, algo falló"));
                        }
                    }
                });
                ResetearInterfaz(false);
                MessageBox.Show($"Proceso terminado. Archivos: {exitos} de {total}", "Finalizado");
                txtStatus.Text = "Listo.";
            }
        }
    }

   // ==========================================
    // VENTANAS DE DIÁLOGO (DISEÑO MEJORADO)
    // ==========================================
    
 public class DialogoPassword : Window
    {
        private PasswordBox pb;
        public string Password { get { return pb.Password; } }

        public DialogoPassword()
        {
            Title = "Seguridad"; 
            Width = 350; 
            Height = 220; // <--- AUMENTADO para que quepa el botón
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            
            StackPanel sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(new TextBlock { Text = "Ingresa la contraseña:", Margin = new Thickness(0,0,0,10), FontSize=14 });
            
            pb = new PasswordBox { FontSize = 14, Padding = new Thickness(5) };
            sp.Children.Add(pb);

            Button btn = new Button { Content = "Aceptar", Margin = new Thickness(0,20,0,0), Height = 35, Cursor = System.Windows.Input.Cursors.Hand };
            btn.Click += (s, e) => { DialogResult = true; };
            sp.Children.Add(btn);

            Content = sp;
        }
    }

    public class DialogoTexto : Window
    {
        private TextBox txt;
        public string TextoIngresado { get { return txt.Text; } }

        public DialogoTexto(string titulo, string valorDefault)
        {
            Title = "Entrada de Datos"; 
            Width = 350; 
            Height = 220; // <--- AUMENTADO
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            
            StackPanel sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(new TextBlock { Text = titulo, Margin = new Thickness(0,0,0,10), FontSize=14 });
            
            txt = new TextBox { Text = valorDefault, FontSize = 14, Padding = new Thickness(5) };
            sp.Children.Add(txt);

            Button btn = new Button { Content = "Aplicar", Margin = new Thickness(0,20,0,0), Height = 35, Cursor = System.Windows.Input.Cursors.Hand };
            btn.Click += (s, e) => { DialogResult = true; };
            sp.Children.Add(btn);

            Content = sp;
        }
    }
}
