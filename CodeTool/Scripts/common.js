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
            $("#divTopPage").css("transform", "translateY(-130px)");

        } else {         //否则隐藏
            $("#goTop").fadeOut();
            $("#divTopPage").css("transform", "translateY(-65px)");
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

    $(".mask").bind("click", function () {
        $("#divTopPage").css("transform", "translateY(-65px)");
        $(this).hide();
    });

    $("#btnTopSearch").bind("click", function () {
        $("#divTopPage").css("transform", "translateY(0)");
        $("#txtTopSearch").focus();
        $(".mask").show();
    });

    $("#txtTopSearch").bind("keydown", function (e) {
        if (e.keyCode != 13) return;
        TopSearch();
    });

    function TopSearch() {
        var pageName = $("#txtTopSearch").val();
        if (pageName.trim() == "") return;
        $.getJSON("/Main/SearchByName",
            {
                PageName: pageName
            }, function (data) {
                if (data != null) {
                    if (data.Message != null) {
                        if (data.Message == "empty") {
                            alert("无相关结果");
                        }
                    } else {
                        location.href = data;
                    }
                }
            });
        //location.href = "/Main/SearchPages?PageName=" + pageName;
    }

}

//初始化ajax进度条插件NProgress
$(document).ajaxStart(function () {
    NProgress.start();
}).ajaxStop(function () {
    NProgress.done();
});
