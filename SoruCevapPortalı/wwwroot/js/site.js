// jQuery hazır olunca çalıştır
$(document).ready(function () {

    console.log("Site.js yüklendi");


    //   SORU OYLAMA (QUESTION VOTE)
   
    $(".question-vote-btn").on("click", function () {

        var questionId = $(this).data("question-id");
        var isUpvote = $(this).data("vote") === "up";
        var token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: "/Question/Vote",
            type: "POST",
            headers: {
                "RequestVerificationToken": token
            },
            data: {
                id: questionId,
                isUpvote: isUpvote
            },
            success: function (result) {
                if (result.success) {
                    $("#question-vote-count").text(result.voteCount);
                } else {
                    alert(result.message || "Bir hata oluştu.");
                }
            },
            error: function () {
                alert("Sunucu hatası!");
            }
        });

    });

    //   SORU DETAY SAYFASINDA CEVAPLARI ÇEKME
   
    if ($("#answersList").length > 0) {

        var questionId = $("#answersList").data("question-id");

        $.ajax({
            url: "/Question/GetAnswers/" + questionId,
            type: "GET",
            success: function (html) {
                $("#answersList").html(html);
            },
            error: function () {
                console.log("Cevaplar yüklenemedi.");
            }
        });

    }

});
