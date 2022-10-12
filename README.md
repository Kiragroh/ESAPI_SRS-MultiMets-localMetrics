# ESAPI_SRS-MultiMets-localMetrics
Simplified version of a ESAPI script to calculate local plan quality metrics for SRS cases with multiple lesions

(Single-File-Plugin for testing on TBox)

First-Compile tips:
- add your own ESAPI-DLL-Files (VMS.TPS.Common.Model.API.dll + VMS.TPS.Common.Model.Types). Usually found in C:\Program Files\Varian\RTM\16.1\esapi\API
- For clinical Mode: Create a binary Plugin script with 'Eclipse Script Wizard'. Copy&Paste the provided code and approve the produced esapi.dll file in External Treatment Planning (Approvement is necessary if 'Is Writeable = true')

Note:
- script is optimized to work with Eclipse 16.1, but should be okay for Eclipse 15 as well
- absolute ESAPI-beginner should first look at my GettingStartedMaterial (collection of many helpful stuff from me or others and even includes a PDF version of some ESAPI-OnlineHelps) https://drive.google.com/drive/folders/1-aYUOIfyvAUKtBg9TgEETiz4SYPonDOO

Comments:
![Test Image 1](https://github.com/Kiragroh/ESAPI_SRS-MultiMets-localMetrics/blob/main/SRS-PlanQuality-Rings.PNG)
