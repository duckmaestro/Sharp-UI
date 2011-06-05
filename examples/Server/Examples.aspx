<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Examples.aspx.cs" Inherits="Server.Examples" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Sharp UI Examples</title>
	<style type="text/css">
		html, body { width:100%; height:100%; }
		body{overflow:auto;}
		html, body, div, h1, h2, h3, h4, h5, h6 { margin:0; padding:0; }    
	</style>
</head>
<body>
	<script src="http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.5.2<%= this.IsDebug ? "" : ".min" %>.js " type="text/javascript"></script>
	<script src="/Scripts-SS/mscorlib<%= this.IsDebug ? ".debug" : "" %>.js" type="text/javascript"></script>
	<script src="/Scripts-SS/SharpUI.Examples<%= this.IsDebug ? ".debug" : "" %>.js<%= "?bypassCache=" + DateTime.Now.ToBinary() %>" type="text/javascript"></script>

	<div id="placeholder"></div>

	
	<script type="text/javascript">
		var application = new Examples.Application("placeholder");
	</script>
</body>
</html>
