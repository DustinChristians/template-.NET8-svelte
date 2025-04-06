<script>
  import { onMount } from "svelte"

  const API_URL = "https://localhost:44333/api/messages"

  let messages = []
  let error = ""

  let newMessage = {
    userId: "",
    text: "",
    channelId: "",
  }

  let editingMessage = null
  let editingData = {
    userId: "",
    text: "",
    channelId: "",
  }

  let usersList = []

  async function fetchUsers() {
    try {
      const res = await fetch("https://localhost:44333/api/users")
      if (!res.ok) {
        error = `Failed to fetch users: ${res.status}`
        return
      }
      usersList = await res.json()
    } catch (err) {
      error = `Error: ${err.message}`
    }
  }

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
    fetchUsers()
  })

  async function sendMessage() {
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

  function startEditing(message) {
    editingMessage = message
    editingData = {
      userId: message.userId,
      text: message.text,
      channelId: message.channelId,
    }
  }

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

  <section>
    <h2>Send a New Message</h2>
    <div>
      <select bind:value={newMessage.userId}>
        <option value="" disabled selected>Select a User</option>
        {#each usersList as user}
          <option value={user.id}>
            {user.firstName}
            {user.lastName} ({user.email})
          </option>
        {/each}
      </select>
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

  <section>
    <h2>All Messages</h2>
    {#if messages.length === 0}
      <p>No messages available.</p>
    {:else}
      <ul>
        {#each messages as message (message.id)}
          <li>
            {#if editingMessage && editingMessage.id === message.id}
              <div>
                <select bind:value={editingData.userId}>
                  <option value="" disabled>Select a User</option>
                  {#each usersList as user}
                    <option value={user.id}>
                      {user.firstName}
                      {user.lastName} ({user.email})
                    </option>
                  {/each}
                </select>
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

  input,
  select {
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
