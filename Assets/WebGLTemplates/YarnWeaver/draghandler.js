function dragover_handler(ev) {
 ev.stopPropagation();
 ev.preventDefault();
}
function drop_handler(ev) {
 ev.preventDefault();
 // Load yarn files
 var filename = ev.dataTransfer.files[0].name;
 var extension = filename.split('.')[1];
 
 dragfeedback.style.visibility = "hidden";
  
 if (extension != "json") // TODO: support other yarn files
  return;
 
 var reader = new FileReader();
 reader.onload = function(e) {
  gameInstance.SendMessage('WebGLDragAndDrop', 'OnDrop', 
   JSON.stringify( { filename: filename, fileContents: e.target.result } )
  );
 }
 reader.readAsText(ev.dataTransfer.files[0]);
}
function dragenter_handler(ev) {
 ev.stopPropagation();
 ev.preventDefault();
 gameInstance.SendMessage('WebGLDragAndDrop', 'OnDragEnter');
 dragfeedback.style.visibility = "visible";
}
function dragexit_handler(ev) {
 gameInstance.SendMessage('WebGLDragAndDrop', 'OnDragExit');
 dragfeedback.style.visibility = "hidden";
}
function dragleave_handler(ev) {
 gameInstance.SendMessage('WebGLDragAndDrop', 'OnDragExit');
 dragfeedback.style.visibility = "hidden";
}
document.addEventListener("DOMContentLoaded", function(event) {
// DOM loaded
var gameContainer = document.getElementById("gameContainer");
gameContainer.addEventListener("drop", drop_handler);
gameContainer.addEventListener("dragover", dragenter_handler);
gameContainer.addEventListener("dragenter", dragenter_handler);
gameContainer.addEventListener("dragexit", dragexit_handler);
gameContainer.addEventListener("dragleave", dragleave_handler);
var dragfeedback = document.getElementById("dragfeedback");
dragfeedback.style.visibility = "hidden";	
var dragtext = document.getElementById("dragtext");
})