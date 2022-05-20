# Introduction
 
At InRule Technology®, our mission is to make automation accessible across the enterprise. Our explainable decision automation and machine learning technology empower anyone to build business logic on a single source of truth and understand all the factors that go into a single prediction - all without sifting through code.
The Machine Learning Extension adds the ability to natively interact with xAI Workbench Machine Learning engines. Unlike traditional machine learning platforms, which deliver a prediction and confidence score or rank feature importance only as they relate to the model itself, our predictions with the why® make it easy for analytics and business teams to apply machine learning quickly and effectively with an easy-to-use workbench and explainable outputs – all without code, and with detailed factors behind every single prediction. The InRule AI-enabled End to End Automation Platform connects domain experts with data science and delivery teams to do just that - while maintaining maximum transparency and explainability.
 
### What is the support model for the Machine Learning Extension?
The Machine Learning (ML) extension is released as a managed extension.  This means that InRule’s support team fully supports it.  Any question or issue may be submitted to [support@inrule.com].

### Licensing
The Machine Learning Extension requires a valid xAI Workbench license.

### System Requirements
* irAuthor version 5.7.2 or newer
* irAuthor System Requirements


### Installation
Installation instructions are located [here](INSTALLATION.md)

### Documentation and Samples:

## Usage of xAI Workbench extension
1. Create a new Machine Learning-interaction
    + Click on the Machine Learning section on the left-hand navigation in irAuthor.
    + Click on the green plus with "Add" at the top of the section.
1. Connect to an instance of XAI Workbench
    + Looking at the contents of your new Machine Learning Model, click on the “Add New…” button under the "Machine Learning Service Connection" section
    + If there is a “Log In” button, you’ll need to use it to authenticate. Once done, you should see your tenants listed in the dropdown list - choose the one you would like to use that has an XAI Workbench associated with it as the relevant instance of xAI Workbench.  It may be automatically selected for you.
        - If your environment does not yet have an instance of xAI Workbench provisioned, navigate to your Portal and use the xAI workbench link at the top of the page to provision it. If there is no such link, contact Inrule support and have them configure your environment to display it.
        - If you would like to connect to a specific xAI Workbench instance using basic authentication, click on the "Explicit Username/Password" section under "Advanced"  in the Machine Learning Endpoint popup, and fill out all fields.
        - If a newly created Tenant is not showing up in the dropdown list, try logging out and back into irAuthor using the Home ribbon "Account" section.
    + Clicking "Save" closes the Machine Learning Endpoint popup and displays your new Machine Learning Endpoint to you. The Model dropdown displays a list of ML models that have been set up on the selected instance of XAI Workbench.
    + If you need to load the XAI Workbench web interface, click on the button to the right of the blue arrow next to the Endpoint.
1. Choose an ML Model that you want to use
    + From the "Model" dropdown list, select a model you would like to interact with during rule execution.
    + If the selected model uses SimClassify+, you can choose to include the Rationale behind the prediction in the output from the ML engine.
    + The "Model Structure" section displays a listing of the various fields used by the selected model.
    + To automatically generate a rule application schema that conforms to the model's structure, select an option from the Data Target dropdown, then click on "Apply ML Model.”
        - If you select an existing Entity, the Entity will have fields added to represent each of the inputs and outputs listed in the Model Structure if needed; existing fields will be left as is.
        - If you select an existing Decision, the Decision will have Inputs and Outputs updated and added to represent each of the inputs and outputs listed in the Model Structure if needed; existing inputs and outputs will be left.
        - If you select a New Entity, a new Entity will be created with fields added to represent each of the inputs and outputs listed in the Model Structure.
        - If you select New Decision, a new Decision will be created with Inputs representing each of the Model Structure Input Fields and Outputs representing the Prediction, Score, and Rationale (if applicable).
1. Create Rules to execute the Machine Learning Prediction
    + From the Entity or Decision, navigate to a RuleSet that exists underneath that ML-Model-conforming item.
    + Right Click on the RuleSet and add a new Machine Learning Prediction.
    + The Inputs and Outputs will be automatically populated with the generated fields during the "Apply ML Model" process.
        - This auto-generation and auto-population of inputs and outputs is done to simplify the process but is not required.  You may map existing fields in your data model into a Machine Learning model's input and output fields and not have a specific Entity or decision representing the ML Model.
    + After the Machine Learning Prediction rule, add any additional rules to consume the prediction, Score, and (if applicable) PredictionRationale.
1. Execute Rules.

## xAI Workbench
+ [Explore xAI Workbench Video and predictions with the why](https://vimeo.com/showcase/8775402/video/592462687)
+ [User Manual](https://support.inrule.com/hc/en-us/sections/4410847709837-xAI-Workbench-User-Manual-v1-8-1)

## Step by step Trial Guide for Combining Decision automation and Machine Learning
+ [Guide](https://docs.inrule.com/author-docs/irauthor/QuickStartXAI.pdf)
