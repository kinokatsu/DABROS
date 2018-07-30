<%@ Page Language="VB" Culture="ja-JP" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="ja">
<head id="Head1" runat="server">
    <title>Javascriptの設定方法について</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=0.8, minimum-scale=0.6, maximum-scale=5.0, user-scalable=yes" />
    <link href="CSS/stl_basic.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
      <h2>Cookieを有効にするための設定</h2>
        <p>&nbsp;</p>
      <p>・Internet Explorerの場合</p>
    <div>
      <ol>
        <li>メニューバーの「ツール」をクリックし、「インターネットオプション」を選択します。</li>
        <li>「プライバシー」のタブを選択し、<strong><span style="font-size: 18pt"></span></strong>下の「詳細設定」ボタンをクリックします。
        </li>
        <li>[自動 Cookie 処理を上書きする]をにチェックを入れます。 </li>
        <li>[ファーストパーティのCookie]および[サードパーティのCookie]で[受け入れる]を選択します。</li>
        <li>一度、ブラウザを閉じて起動しなおしてください。</li>
      </ol>
    </div>
      <p>&nbsp;</p>
      <p>・Firefoxの場合</p>
      <ol>
        <li>メニューバーの「ツール」をクリックし、「オプション」をクリックします。</li>
        <li>「プライバシー」タブを選択し、[Firefoxに] を [履歴を記憶させる」もしくは[記憶させるりれr機を詳細設定する]に設定します</li>
        <li>[サイトから送られてきたCookieを保存する]、[サードパーティのCookieも保存する]にチェックを入れます。</li>
        <li>一度、ブラウザを閉じて起動しなおしてください。</li>
      </ol>
      <p>&nbsp;</p>
      <p>・Google Chromeの場合</p>
      <ol>
        <li>右上のレンチマークをクリックし、「オプション」を選択します。</li>
        <li>「高度な設定」タブを選択し、「プライバシ」の[コンテンツの設定]をボタンをクリックします。 </li>
        <li>「Cookie」で、「ローカルへのデータ設定を許可する」を選択します。 </li>
        <li>一度、ブラウザを閉じて起動しなおしてください。</li>
      </ol>
      <p>&nbsp;</p>
    </form>
</body>
</html>
