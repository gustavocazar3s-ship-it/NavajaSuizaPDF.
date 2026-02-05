using System;
using System.Reflection;

namespace NavajaSuizaPDF.Logica
{
    public class GestorWord
    {
        // ==========================================
        // FUNCIÓN 1: WORD A PDF
        // ==========================================
        public void ConvertirWordAPdf(string rutaOrigen, string rutaDestino)
        {
            Type tipoWord = Type.GetTypeFromProgID("Word.Application");
            if (tipoWord == null) throw new Exception("Error: No se detectó Microsoft Word instalado.");

            dynamic wordApp = Activator.CreateInstance(tipoWord);

            try
            {
                wordApp.Visible = false;
                wordApp.ScreenUpdating = false;

                dynamic wordDoc = wordApp.Documents.Open(rutaOrigen, ReadOnly: true);

                try
                {
                    // 17 = wdExportFormatPDF
                    wordDoc.ExportAsFixedFormat(rutaDestino, 17);
                }
                finally
                {
                    wordDoc.Close(0); // Cerrar sin guardar
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error interno de Word: " + ex.Message);
            }
            finally
            {
                if (wordApp != null) wordApp.Quit(0);
            }
        }

        // ==========================================
        // FUNCIÓN 2: PDF A WORD (¡NUEVA!)
        // ==========================================
        public void ConvertirPdfAWord(string rutaPdf, string rutaDocx)
        {
            Type tipoWord = Type.GetTypeFromProgID("Word.Application");
            if (tipoWord == null) throw new Exception("Error: No se detectó Microsoft Word instalado.");

            dynamic wordApp = Activator.CreateInstance(tipoWord);

            try
            {
                wordApp.Visible = false;
                wordApp.ScreenUpdating = false;
                
                // TRUCO VITAL: Desactivar alertas para que no pregunte al abrir el PDF
                wordApp.DisplayAlerts = 0; // 0 = wdAlertsNone

                // Abrimos el PDF (Word lo convierte internamente al abrirlo)
                dynamic wordDoc = wordApp.Documents.Open(FileName: rutaPdf, ConfirmConversions: false, ReadOnly: true);

                try
                {
                    // Guardamos como documento de Word por defecto (.docx)
                    // 16 = wdFormatDocumentDefault
                    wordDoc.SaveAs2(FileName: rutaDocx, FileFormat: 16);
                }
                finally
                {
                    wordDoc.Close(0);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al convertir PDF: " + ex.Message);
            }
            finally
            {
                // Aseguramos cerrar Word
                if (wordApp != null) wordApp.Quit(0);
            }
        }
        // ==========================================
        // FUNCIÓN 3: COMPRIMIR PDF (¡NUEVA!)
        // ==========================================
        public void ComprimirPdf(string rutaOrigen, string rutaDestino)
        {
            Type tipoWord = Type.GetTypeFromProgID("Word.Application");
            if (tipoWord == null) throw new Exception("Error: No se detectó Microsoft Word.");

            dynamic wordApp = Activator.CreateInstance(tipoWord);

            try
            {
                wordApp.Visible = false;
                wordApp.ScreenUpdating = false;
                wordApp.DisplayAlerts = 0;

                // Abrimos el PDF original
                dynamic wordDoc = wordApp.Documents.Open(FileName: rutaOrigen, ConfirmConversions: false, ReadOnly: true);

                try
                {
                    // EL TRUCO: Exportamos con OptimizeFor = 1 (wdExportOptimizeForOnScreen)
                    // Esto baja la calidad de las imágenes para reducir el peso
                    wordDoc.ExportAsFixedFormat(
                        OutputFileName: rutaDestino, 
                        ExportFormat: 17, // PDF
                        OpenAfterExport: false,
                        OptimizeFor: 1 // <--- 0=Impresión (Pesado), 1=Pantalla (Ligero)
                    );
                }
                finally
                {
                    wordDoc.Close(0);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al comprimir: " + ex.Message);
            }
            finally
            {
                if (wordApp != null) wordApp.Quit(0);
            }
        }

    }
}