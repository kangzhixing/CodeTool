window.onload = function () {
    var clientHeight = document.documentElement.clientHeight;   //获取可视区域的高度
    var timer = null; //定义一个定时器
    var isTop = true; //定义一个布尔值，用于判断是否到达顶部

    window.onscroll = function (event, i) {         //滚动条滚动事件
        if ($(".mask").css("display") != "none") {
            return;
        }

        //获取滚动条的滚动高度
        var osTop = document.documentElement.scrollTop || document.body.scrollTop;

        var limitHeight = 60;
        if (osTop >= limitHeight) {  //如果滚动高度大于限制高度，则显示回到顶部按钮
            //if (osTop >= clientHeight) {  //如果滚动高度大于可视区域高度，则显示回到顶部按钮
            $("#goTop").fadeIn();
            $("#divTopPage").css("top", "-130px");            //$("#divTopPage").css("transform", "translateY(-130px)");
            $(".divTop").css("box-shadow", "0 0 5px grey");

        } else {         //否则隐藏
            $("#goTop").fadeOut();
            $("#divTopPage").css("top", "-65px");            //$("#divTopPage").css("transform", "translateY(-65px)");
            $(".divTop").css("box-shadow", "");
        }
        //主要用于判断当 点击回到顶部按钮后 滚动条在回滚过程中，若手动滚动滚动条，则清除定时器
        if (!isTop) {

            clearInterval(timer);
        }
        isTop = false;
    }

    $(".divTitleText").text(window.document.title);

    $("#divTitlePart .divTitleText").bind("click", function () { GoToPageTop() });

    $(".goTop").bind("click", function () {//回到顶部按钮点击事件
        GoToPageTop();
    });

    function GoToPageTop() {
        clearInterval(timer);
        //设置一个定时器
        timer = setInterval(function () {
            //获取滚动条的滚动高度
            var osTop = document.documentElement.scrollTop || document.body.scrollTop;
            //用于设置速度差，产生缓动的效果
            var speed = Math.floor(-osTop / 6);
            document.documentElement.scrollTop = document.body.scrollTop = osTop + speed;
            isTop = true;  //用于阻止滚动事件清除定时器
            if (osTop == 0) {
                clearInterval(timer);
            }
        }, 30);
    }

    $("#btnSearchPages").bind("click", function () {
        TopSearch();
    });

    $("#btnMenu").bind("click", function () {
        location.href = "/Main/SearchPagesByKeyword?isall=1";
    });

    $(".mask").bind("click", function () {
        $("#divTopPage").css("top", "-65px");        //$("#divTopPage").css("transform", "translateY(-65px)");
        $(this).hide();
    });

    $("#btnGetSearchPart").bind("click", function () {
        $("#divTopPage").css("top", "0px");        //$("#divTopPage").css("transform", "translateY(0)");
        $("#txtTopSearch").focus();
        $(".mask").show();
    });

    $("#txtTopSearch").bind("keydown", function (e) {
        if (e.keyCode != 13) return;
        TopSearch();
    });

    //$("body").click(function (e) {

    //    AddPoint(e.pageX, e.pageY)
    //});

    console.log("%cWelcome to use CodeTool", " text-shadow: 0 1px 0 #ccc,0 2px 0 #c9c9c9,0 3px 0 #bbb,0 4px 0 #b9b9b9,0 5px 0 #aaa,0 6px 1px rgba(0,0,0,.1),0 0 5px rgba(0,0,0,.1),0 1px 3px rgba(0,0,0,.3),0 3px 5px rgba(0,0,0,.2),0 5px 10px rgba(0,0,0,.25),0 10px 10px rgba(0,0,0,.2),0 20px 20px rgba(0,0,0,.15);font-size:3em")
}

//页面搜索
function TopSearch(pageName) {
    $("#txtTopSearch").focus();
    if (pageName == null)
        pageName = $("#txtTopSearch").val();
    if (pageName.trim() == "") return;

    $.ajax({
        url: "/Main/SearchByName",
        asycn: false,
        method: "get",
        dataType: "json",
        data: {
            PageName: pageName
        },
        success: function (data) {
            if (data != null) {
                if (data.Message != null) {
                    if (data.Message == "empty") {
                        alert("无相关结果");
                    }
                } else {
                    location.href = data;
                }
            }
        }
    });
    //location.href = "/Main/SearchPages?PageName=" + pageName;
}

//初始化ajax进度条插件NProgress
$(document).ajaxStart(function () {
    NProgress.start();
}).ajaxStop(function () {
    NProgress.done();
});

//语音播放
var playTime = 0;
function SpeenchText(text) {
    if (playTime++ == 0) { // 解决Safari不自动播放
        $("#adoPlayer")[0].play();
    }

    $.getJSON("/Tool/GetAudio", {
        tex: text,
        per: $("#sltPer").find("option:selected").val()
    }, function (data) {
        if (data != null && data.Message != null) {
            alert(data.Message);
        }
        else {
            $("#adoPlayer").attr("src", "..\\" + data);
            $("#adoPlayer")[0].play();
        }
    });
}