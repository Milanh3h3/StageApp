const connection = new signalR.HubConnectionBuilder()
    .withUrl("/feedbackHub")
    .build();

connection.on("ReceiveFeedback", function (message) {
    const feedbackDiv = document.getElementById("feedback");

    const newFeedback = document.createElement("div");
    newFeedback.classList.add("feedback-message");
    newFeedback.textContent = message;

    feedbackDiv.prepend(newFeedback);

    setTimeout(() => {
        newFeedback.remove();
    }, 30000);
});

connection.start().catch(function (err) {
    console.error(err.toString());
});
