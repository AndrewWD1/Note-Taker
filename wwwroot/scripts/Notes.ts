interface INote {
  id: number;
  text: string;
  createdAt: Date;
  complete: boolean;
}

(function () {
  // IIFE to get variables out of global scope

  var list = document.querySelector("#table");
  let notes: INote[];

  const deleteNote = (id: number) => {
    let currentNoteCompleteColumn = document.querySelector(
      `#id_${id} .delete-column`
    );

    if (currentNoteCompleteColumn) {
      currentNoteCompleteColumn.innerHTML = "&#9480;";

      fetch("/api/notes/deletenote", {
        method: "POST",
        headers: {
          Authorization: `Bearer ${sessionStorage.token}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          noteId: id,
          userId: sessionStorage.getItem("userId"),
        }),
      })
        .then((x: any) => x.json())
        .then((fetchedNotes: INote[]) => {
          notes = fetchedNotes;
          renderNotes();
        });
    }
  };

  const createNote = () => {
    let text = document.querySelector("input")?.value;

    if (!text || text.length < 1) return;

    fetch("/api/notes/createnote", {
      method: "POST",
      headers: {
        Authorization: `Bearer ${sessionStorage.token}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        text: text,
        userId: sessionStorage.getItem("userId"),
      }),
    })
      .then((x) => x.json())
      .then((fetchedNotes) => {
        notes = fetchedNotes;
        renderNotes();
      });
  };

  const toggleNoteComplete = (note: INote) => {
    let currentNoteCompleteColumn = document.querySelector(
      `#id_${note.id} .complete-column`
    );
    if (currentNoteCompleteColumn) {
      currentNoteCompleteColumn.innerHTML = "&#9480;";

      fetch("/api/notes/toggleNoteComplete", {
        method: "PUT",
        headers: {
          Authorization: `Bearer ${sessionStorage.token}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          noteId: note.id,
          userId: sessionStorage.getItem("userId"),
          newToggleState: !note.complete,
        }),
      })
        .then((x) => x.json())
        .then((fetchedNotes) => {
          notes = fetchedNotes;
          renderNotes();
        });
    }
  };

  const renderNotes = (completion?: boolean) => {
    [...(list?.children as any)].forEach((child) => {
      list?.removeChild(child);
    });
    notes.forEach((element) => {
      if (completion === undefined || element.complete === completion) {
        let tr = document.createElement("tr");
        // Give it an id to be able to select for it from the dom
        tr.id = "id_" + element.id;

        // Create the X dingbat and attach the deleteNote function to it
        let x = document.createElement("td");
        x.innerHTML = "&#10005;";
        x.style.cursor = "pointer";
        // Give it a class name in order to be able to select for it for updates
        x.className = "delete-column";
        x.onclick = () => deleteNote(element.id);

        // Create the text
        let textTD = document.createElement("td");
        let text = document.createTextNode(element.text);
        textTD.appendChild(text);

        // Create the complete element
        let y = document.createElement("td");
        // Give it a class name in order to be able to select for it for updates
        y.className = "complete-column";
        if (element.complete) {
          y.innerHTML = "&#10003;";
        }
        y.style.cursor = "pointer";
        y.onclick = () => toggleNoteComplete(element);

        // Attach delete, text and completion to list item
        tr.appendChild(x);
        tr.appendChild(textTD);
        tr.appendChild(y);

        // append new table element to table
        list?.appendChild(tr);
      }
    });
  };

  fetch(`/api/notes/Get?id=${sessionStorage.getItem("userId")}`, {
    headers: {
      Authorization: `Bearer ${sessionStorage.token}`,
    },
  })
    .then((x) => x.json())
    .then((fetchedNotes) => {
      notes = fetchedNotes;
      renderNotes();
    });

  // Attach event handler to filterers
  const filterCompleteButton: any = document.querySelector(
    "#filter-complete-button"
  );
  if (filterCompleteButton)
    filterCompleteButton.onclick = () => renderNotes(true);

  // Attach event handler to filterers
  const filterTodoButton: any = document.querySelector("#filter-todo-button");
  if (filterTodoButton) filterTodoButton.onclick = () => renderNotes(false);

  // Attach event handler to filterers
  const showAllButton: any = document.querySelector("#show-all-button");
  if (showAllButton) showAllButton.onclick = () => renderNotes();

  // Attach event handler to note creator
  const createNoteButton: any = document.querySelector("#create-note-button");
  if (createNoteButton) createNoteButton.onclick = createNote;
})();
