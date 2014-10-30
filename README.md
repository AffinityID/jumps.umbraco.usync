uSync for Umbraco 
===================

Syncing tool for umbraco to read and write the database elements to disk.

Objective
---------
The aim of uSync is to make disk based version control of Umbraco possible. 

Background
-----------
Umbraco has a lot of settings stored on disk, and a lot stored in the database. You can version control the
files quite easily but the database changes are a pain. 

uSync attaches itself to the save events inside Umbraco and writes out files containing the settings db settings
for key things when they are saved. 

For example, saving a document type with uSync installed will result in a .xml file been written to the uSync folder
of your Umbraco installation. 

When the web site is restarted, uSync will read from the disk and add any xml files it finds back into the database.

In theory this can help you: 

1. Source Control your umbraco developements
2. automate some elements of the deployment of umbraco
3. keep multiple developers in sync without sharing the database

Status
======
uSync 3.x is the result of a major investment in the codebase of uSync, and includes lots of new features and fixes making it the most complete version fro far:

v3.0 was build specifically for v6.1.6 of umbraco but works all the way upto v6.2.4 (so far).

this branch will be moved forward overtime and become the stable root for v7 versions of uSync replacing v2.2.
   



