var instance = 'https://login.microsoftonline.com/'; // Azure AD logon endpoint
var dateform = "DD/MM/YYYY"; // UK locale
var timeform = "HH:mm"; // 12h UK time

var namesplitter = "officehours";
// Figure out what the current URI is and where to get our config from.
var serverprefixaddress = window.location.href.split("/")[2].split(".")[0];
var prefix = serverprefixaddress.split(namesplitter)[0]
var resource = 'https://' + prefix + 'ukohfn.azurewebsites.net';
var endpoint = 'https://' + prefix + '-ukofficehours.azurewebsites.net/';
var rootfnsite = resource + "/";