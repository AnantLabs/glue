<%@ Page Language="C#" CodeBehind="Default.aspx.cs" Inherits="web._Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>Glue.Web.Admin sample application</title>
    <meta http-equiv="Content-type" content="text/html;charset=UTF-8" />
    <link href="css/default.css" type="text/css" rel="stylesheet" />
</head>
<body>
    <p><img src="gfx/grondwerk-it-logo-192.png" width="192" height="161" alt="Grondwerk IT" /></p>
    <p class="footer">
      © 2009-2032 Grondwerk IT<br />
      Application version: <%= appVersion %><br />
      Database schema version: <%= appSchemaVersion %><br />
      Configuration version: <%= appConfigVersion %>
    </p>
</body>
</html>