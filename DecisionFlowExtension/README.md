# Introduction
Decisions created within InRule may require multiple logical steps over the course of their execution.  The Decision Flow extension allows Rule Authors to create a process flow for a Decision in a visual editor, which is then used to generate the required logical and rule set structures to facilitate the rule authoring process.
 
## What is the support model for the Decision Flow Extension?
The Decision Flow extension is discontinued as a Managed Extension until further notice

## Licensing
The Decision Flow extension requires only a valid irAuthor Authoring license.

## System Requirements
* irAuthor version 5.8 or newer
* irAuthor System Requirements

## Installation
This extension can be installed using the Extension Manager found within irAuthor version 5.8 and newer.

## Documentation and Samples:
When working on a Decision, you can launch the Decision Flow editor either from the context (right-click) menu, or from the Decision Flow tab in the navigation ribbon.  Once in the editor, you're able to add a number of different element types and connections between them to build out your application's logical flow, and then Apply them to enact the changes in your rule application.

Available Elements:
- Start (green circle): Initiation of the Decision flow; only one may be created for a given Flow
- Gateway (orange diamond): An exclusive gateways represents a yes / no decision, i.e.; a decision with only one path out from the gateway
- Rule Set (blue rectangle): Represents a rule set that is being executed
- End (red circle): The end of the decision flow that terminates all other paths
- Transition (arrow between elements): The transition arrow represents the path the decision flow travels during execution
- Annotation (bracket): Annotations are used for comments, explanations, KPIs or other metadata
