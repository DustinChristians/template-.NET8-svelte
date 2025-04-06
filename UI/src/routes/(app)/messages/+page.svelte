<script>
  import { onMount } from "svelte"

  // Update this with the correct API URL
  const API_URL = "https://localhost:44333/api/messages"

  // The messages returned by the API (which include Id, Guid, UserId, Text, ChannelId, etc.)
  let messages = []
  let error = ""

  // New message fields matching the CreateMessage model
  let newMessage = {
    userId: "",
    text: "",
    channelId: "",
  }

  // For editing an existing message, we'll store the current message and a copy of the editable fields
  let editingMessage = null
  let editingData = {
    userId: "",
    text: "",
    channelId: "",
  }

  // Fetch messages from the API on component mount
  async function fetchMessages() {
    try {
      const res = await fetch(API_URL)
      if (!res.ok) {
        error = `Failed to fetch messages: ${res.status}`
        return
      }
      messages = await res.json()
    } catch (err) {
      error = `Error: ${err.message}`
    }
  }

  onMount(() => {
    fetchMessages()
  })

  // Create a new message using the CreateMessage model
  async function sendMessage() {
    // Ensure required fields are not empty (basic client-side validation)
    if (!newMessage.userId || !newMessage.text.trim() || !newMessage.channelId)
      return

    try {
      const res = await fetch(API_URL, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          userId: Number(newMessage.userId),
          text: newMessage.text,
          channelId: Number(newMessage.channelId),
        }),
      })
      if (!res.ok) {
        error = `Failed to send message: ${res.status}`
        return
      }
      // Reset the new message fields
      newMessage = { userId: "", text: "", channelId: "" }
      await fetchMessages()
    } catch (err) {
      error = `Error: ${err.message}`
    }
  }

  // Enter edit mode for a message and copy its values into editingData
  function startEditing(message) {
    editingMessage = message
    editingData = {
      userId: message.userId,
      text: message.text,
      channelId: message.channelId,
    }
  }

  // Update a message using the UpdateMessage model via PUT endpoint
  async function updateMessage() {
    if (
      !editingData.userId ||
      !editingData.text.trim() ||
      !editingData.channelId
    )
      return

    try {
      const res = await fetch(`${API_URL}/${editingMessage.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          userId: Number(editingData.userId),
          text: editingData.text,
          channelId: Number(editingData.channelId),
        }),
      })
      if (!res.ok) {
        error = `Failed to update message: ${res.status}`
        return
      }
      editingMessage = null
      editingData = { userId: "", text: "", channelId: "" }
      await fetchMessages()
    } catch (err) {
      error = `Error: ${err.message}`
    }
  }

  // Delete a message (only the id is needed)
  async function deleteMessage(id) {
    try {
      const res = await fetch(`${API_URL}/${id}`, {
        method: "DELETE",
      })
      if (!res.ok) {
        error = `Failed to delete message: ${res.status}`
        return
      }
      await fetchMessages()
    } catch (err) {
      error = `Error: ${err.message}`
    }
  }
</script>

<main>
  <h1>Messages</h1>

  {#if error}
    <p class="error">{error}</p>
  {/if}

  <!-- Section to send a new message -->
  <section>
    <h2>Send a New Message</h2>
    <div>
      <input
        type="number"
        bind:value={newMessage.userId}
        placeholder="User ID"
      />
    </div>
    <div>
      <input
        type="text"
        bind:value={newMessage.text}
        placeholder="Type your message here"
      />
    </div>
    <div>
      <input
        type="number"
        bind:value={newMessage.channelId}
        placeholder="Channel ID"
      />
    </div>
    <button on:click={sendMessage}>Send</button>
  </section>

  <!-- Section displaying all messages -->
  <section>
    <h2>All Messages</h2>
    {#if messages.length === 0}
      <p>No messages available.</p>
    {:else}
      <ul>
        {#each messages as message (message.id)}
          <li>
            {#if editingMessage && editingMessage.id === message.id}
              <!-- Edit mode: allow updating all fields -->
              <div>
                <input
                  type="number"
                  bind:value={editingData.userId}
                  placeholder="User ID"
                />
              </div>
              <div>
                <input
                  type="text"
                  bind:value={editingData.text}
                  placeholder="Text"
                />
              </div>
              <div>
                <input
                  type="number"
                  bind:value={editingData.channelId}
                  placeholder="Channel ID"
                />
              </div>
              <button on:click={updateMessage}>Update</button>
              <button on:click={() => (editingMessage = null)}>Cancel</button>
            {:else}
              <!-- Display mode -->
              <div>
                <strong>Message:</strong>
                {message.text}
              </div>
              <div>
                <small
                  >User ID: {message.userId} | Channel ID: {message.channelId}</small
                >
              </div>
              <div>
                <small
                  >Created: {new Date(
                    message.createdOn,
                  ).toLocaleString()}</small
                >
              </div>
              <button on:click={() => startEditing(message)}>Edit</button>
              <button on:click={() => deleteMessage(message.id)}>Delete</button>
            {/if}
          </li>
        {/each}
      </ul>
    {/if}
  </section>
</main>

<style>
  main {
    max-width: 800px;
    margin: 0 auto;
    padding: 1rem;
    font-family: Arial, sans-serif;
  }

  input {
    padding: 0.5rem;
    margin: 0.25rem 0;
    width: 100%;
    box-sizing: border-box;
  }

  button {
    margin-right: 0.5rem;
    margin-top: 0.5rem;
  }

  .error {
    color: red;
  }

  ul {
    list-style-type: none;
    padding: 0;
  }

  li {
    margin: 1rem 0;
    padding: 0.5rem;
    border: 1px solid #ddd;
    border-radius: 4px;
  }
</style>
