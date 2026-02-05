using System;
using System.Threading.Tasks;
using System.Windows;

namespace NavajaSuizaPDF
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            IniciarCarga();
        }

        private async void IniciarCarga()
        {
            // 1. Esperamos 3 segundos (simulando carga de módulos)
            await Task.Delay(3000);

            // 2. Abrimos la Navaja Suiza real
            MainWindow main = new MainWindow();
            main.Show();

            // 3. Cerramos esta pantalla de carga
            this.Close();
        }
    }
}