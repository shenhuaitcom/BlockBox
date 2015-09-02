<%@ Page Language="C#" %>

<!DOCTYPE html>
<html>
<head>
	<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
	<title>支付宝</title>
</head>
<body>
	<%
		SH.Alipay alipay = new SH.Alipay();
	%>
	<%=alipay.Royalty_Parameters("15969782256@163.com^0.01^傅晨晨给你钱|conanmiao@qq.com^0.01^苗，这是你的钱").PayDirect(1)%>
</body>
</html>
