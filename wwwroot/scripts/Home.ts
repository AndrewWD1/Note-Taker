interface INote {
    id: number;
    text: string;
    createdAt: Date;
    complete: boolean;
}

(function () {
    // Set up of document
    let tableWindow = document.querySelector("#table-window");

    let table = document.createElement("table");
    table.className = "table table-hover";
    table.innerHTML = `<thead><tr><th scope="col">Delete</th><th scope="col">Note</th><th scope="col">Complete</th></tr></thead><tbody id="table"></tbody>`;

    let createButton = document.createElement("button");
    createButton.id = "create-note-button";
    createButton.style.marginRight = "5px";
    createButton.appendChild(document.createTextNode("Create Note"));
    let input = document.createElement("input");
    input.value = "New Note";

    tableWindow?.appendChild(table);
    tableWindow?.appendChild(createButton);
    tableWindow?.appendChild(input);

    // Creating functions to implement UI
    const simulateServerLatency = () => {
        let latencyFactor = 500 * Math.random();

        return Math.random() > 0.5 ? 600 + latencyFactor : 500 - latencyFactor;
    };

    let list = document.querySelector("#table");

    let notes: INote[] = [];

    const deleteNote = (id: number) => {
        let currentNoteCompleteColumn = document.querySelector(
            `#id_${id} .delete-column`
        );

        if (currentNoteCompleteColumn) {
            currentNoteCompleteColumn.innerHTML = "&#9480;";

            setTimeout(() => {
                notes = notes.filter((note) => note.id !== id);
                renderNotes();
            }, simulateServerLatency());
        }
    };

    let count = 0;
    let nextId = function () {
        count += 1;
        return count;
    };
    const createNote = () => {
        let text = document.querySelector("input")?.value;

        if (!text || text.length < 1) return;

        let newNote: INote = {
            id: nextId(),
            text: text,
            createdAt: new Date(),
            complete: false,
        };

        setTimeout(() => {
            notes.push(newNote);
            renderNotes();
        }, simulateServerLatency());
    };

    const toggleNoteComplete = async (note: INote) => {
        let currentNoteCompleteColumn = document.querySelector(
            `#id_${note.id} .complete-column`
        );

        if (currentNoteCompleteColumn) {
            currentNoteCompleteColumn.innerHTML = "&#9480;";
            await setTimeout(() => {
                note.complete = !note.complete;
                if (currentNoteCompleteColumn) {
                    if (note.complete) currentNoteCompleteColumn.innerHTML = "&#10003;";
                    else currentNoteCompleteColumn.innerHTML = "";
                }
            }, simulateServerLatency());

            return;
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

    /**
     * Function to initaite an animation typing and rendering a note
     * @param note
     */
    const typeAndRenderNote = (note: INote): Promise<void> => {
        return new Promise((res) => {
            let g = setupStringGen(note.text)();
            input.value = "";
            let t = g.next().value;
            const interval = setInterval(() => {
                input.value += t;
                t = g.next().value;
                if (!t) {
                    clearInterval(interval);
                    notes.push(note);
                    renderNotes();
                    res();
                }
            }, 100 + 60 * Math.random());
        });
    };

    /**
     * Creates generator that returns the charachters of a string suquentially
     * @param str
     */
    const setupStringGen = (str: string) => {
        return function* gen() {
            for (let char of str) yield char;
        };
    };

    // Push in the first notes
    let firstNote = {
        id: nextId(),
        text: "This is the notes section available for you to use",
        createdAt: new Date(),
        complete: true,
    };
    notes.push(firstNote);
    renderNotes();

    // Type and render the rest of the notes in sequence
    (async function typingAnimation() {
        await typeAndRenderNote({
            id: nextId(),
            text: "Register",
            createdAt: new Date(),
            complete: false,
        });

        await typeAndRenderNote({
            id: nextId(),
            text: "Sign in, then you can",
            createdAt: new Date(),
            complete: false,
        });

        await typeAndRenderNote({
            id: nextId(),
            text: "Store your notes for later!",
            createdAt: new Date(),
            complete: true,
        });

        input.value = "New Note";

        /**
         * Wait until animation is complete to attach functionlity to buttons
         */
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
})();