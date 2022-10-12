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
- in the background the script creates 4 rings (2 small and 2 bigger rings) for calculation of dose metrics (will be automatically deleted at the end). Reason: example -> when the 50%-isodose is not the same in the two bigger rings, I assume dose bridging and will not calculate metrics to prevent misreporting
- 
![Test Image 1](https://github.com/Kiragroh/ESAPI_SRS-MultiMets-localMetrics/blob/main/SRS-PlanQuality-Rings.PNG)
- the script calculates metrics for all ptvs that have a bigger median dose as prescription (prescription will be separately overwritten if a referencepointDose is set)
- the script result will be a simple MessageBox:
- 
![Test Image 2](https://github.com/Kiragroh/ESAPI_SRS-MultiMets-localMetrics/blob/main/SRS-PlanQuality-MessageBox-Skript.PNG)
- to make the script more usful you can develop a GUI script that writes the provided results or even more to a database. Example screenshot of the GUI-version:
- 
![Test Image 3](https://github.com/Kiragroh/ESAPI_SRS-MultiMets-localMetrics/blob/main/SRS-PlanQuality-GUI-Skript.PNG)

