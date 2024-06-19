using System.Dynamic;
using System.Security.Policy;
using DotLiquid;
using Newtonsoft.Json;
using Hash = DotLiquid.Hash;

namespace LiquidTemplateEvaluator
{
    public partial class LiquidTemplateEvaluatorForm : Form
    {
        private const string lastUsedTemplateFileName = "template.txt";
        private const string lastUsedInputDataJsonFileName = "inputData.json";

        public LiquidTemplateEvaluatorForm()
        {
            InitializeComponent();
        }

        private void Evaluate()
        {
            if (string.IsNullOrWhiteSpace(txtDataJson.Text)
                && string.IsNullOrWhiteSpace(txtTemplateJson.Text))
            {
                txtOutput.Text = "Waiting for the liquid template and input json";
                return;
            }
            else if (string.IsNullOrWhiteSpace(txtDataJson.Text))
            {
                txtOutput.Text = "Waiting for the input json";
                return;
            }
            else if (string.IsNullOrWhiteSpace(txtTemplateJson.Text))
            {
                txtOutput.Text = "Waiting for the liquid template";
                return;
            }

            Template template;
            try
            {
                template = Template.Parse(txtTemplateJson.Text);
            }
            catch (Exception templateError)
            {
                txtOutput.Text = "Invalid liquid template. Error=> "
                    + Environment.NewLine + Environment.NewLine + templateError;
                return;
            }

            ExpandoObject jsonObject;
            try
            {
                jsonObject = JsonConvert.DeserializeObject<ExpandoObject>(txtDataJson.Text);
            }
            catch (Exception jsonError)
            {
                txtOutput.Text = "Invalid input data json. Error=> "
                    + Environment.NewLine + Environment.NewLine + jsonError;
                return;
            }

            Hash jsonHash;
            try
            {
                jsonHash = Hash.FromDictionary(jsonObject);
            }
            catch (Exception dictionaryConversionError)
            {
                txtOutput.Text = "Input json could not be converted into dictionary. Error=> "
                    + Environment.NewLine + Environment.NewLine + dictionaryConversionError;
                return;
            }

            try
            {
                var result = template.Render(jsonHash);
                txtOutput.Text = result;
                SaveLastUsedData();
            }
            catch (Exception exp)
            {
                txtOutput.Text = "Could not evaluate. Error => "
                    + Environment.NewLine + Environment.NewLine + exp.ToString();
            }
        }

        private void SaveLastUsedData()
        {
            File.WriteAllText(lastUsedTemplateFileName, txtTemplateJson.Text);
            File.WriteAllText(lastUsedInputDataJsonFileName, txtDataJson.Text);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!LoadFiles())
            {
                LoadSampleFiles();
            }

            Evaluate();
        }

        private bool LoadFiles()
        {
            return LoadFiles(lastUsedTemplateFileName, lastUsedInputDataJsonFileName);
        }

        private bool LoadFiles(string templateFile, string dataJsonFile)
        {
            bool loadResult = true;
            if (File.Exists(templateFile))
            {
                txtTemplateJson.Text = File.ReadAllText(templateFile);
            }
            else
            {
                loadResult = false;
            }

            if (File.Exists(dataJsonFile))
            {
                txtDataJson.Text = File.ReadAllText(dataJsonFile);
            }
            else
            {
                loadResult = false;
            }

            return loadResult;
        }

        private void txtDataJson_TextChanged(object sender, EventArgs e)
        {
            Evaluate();
        }

        private void txtTemplateJson_TextChanged(object sender, EventArgs e)
        {
            Evaluate();
        }

        private void loadSampleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadSampleFiles();
        }

        private void LoadSampleFiles()
        {
            if ((string.IsNullOrWhiteSpace(txtTemplateJson.Text) && string.IsNullOrWhiteSpace(txtDataJson.Text))
                            || MessageBox.Show("Are you sure to load/overwrite?") == DialogResult.OK)
            {
                LoadFiles(Path.Combine("sampleData", lastUsedTemplateFileName),
                    Path.Combine("sampleData", lastUsedInputDataJsonFileName));
            }
        }
    }
}