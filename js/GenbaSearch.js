var searchWord = function(){
  var searchResult,
      searchText = $("#q").val(), // 検索ボックスに入力された値
      targetText,
      targetId;
      //hitNum,

  // 検索結果を格納するための配列を用意
  searchResult = [];

  // 検索結果エリアの表示を空にする
  $("#search-result__list").empty();
  $(".search-result__hit-num").empty();

  // 検索ボックスに値が入ってる場合
  if (searchText !== "") {
    $("#RBLConstructionList label").each(function() {
    targetText = $(this).text();
    /* targetId = $(this).context.htmlFor;  htmlFor; jQuery3で使用不可 */
    /* targetId = $(this)[0].control.id;    これでも行けるが添え字0はいまいちなので下ので決定 */
    targetId = $(this).attr("for");
      // 検索対象となるリストに入力された文字列が存在するかどうかを判断
      $("#"+targetId).parent("td").css("background-color","");
      if (targetText.indexOf(searchText) != -1) {
        // 存在する場合はそのリストのテキストを用意した配列に格納
        //searchResult.push(targetText);
        searchResult.push(targetId);
      }
    });

    var position;
    if (searchResult.length !== 0) {
        position = $("#"+searchResult[0]).offset().top;
        position -= 10;
    } else {
        position = 0;
    }

    // 検索結果をページに出力
    for (var i = 0; i < searchResult.length; i ++) {
      //$('<span>').text(searchResult[i]).appendTo('#search-result__list');
        $("#"+searchResult[i]).parent("td").css("background-color","Yellow");
    }

     $("html,body").animate({
        scrollTop : position
    }, {
        duration:1200,
        easing:"easeOutQuad",
        queue : false}
    );
    //if (searchResult.length == 1) {
        $("#"+searchResult[0]).attr("checked","checked");
        //$('#'+searchResult[0]).prop('checked',true);
    //}

    // ヒットの件数をページに出力
    //hitNum = '<span>検索結果</span>：' + searchResult.length + '件見つかりました。';
    //$('.search-result__hit-num').append(hitNum);
  }
};
