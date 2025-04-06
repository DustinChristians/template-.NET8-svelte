<script>
  import { onMount } from "svelte"
  const API_URL = "https://localhost:44333/api/messages"
  let messages = []
  let error = ""
  let newMessage = { userId: "", text: "", channelId: "" }
  let editingMessage = null
  let editingData = { userId: "", text: "", channelId: "" }
  let usersList = []
  let openDropdownId = null
  $: sortedMessages = messages
    .slice()
    .sort((a, b) => new Date(a.createdOn) - new Date(b.createdOn))
  function getUserName(userId) {
    const user = usersList.find((u) => u.id === userId)
    return user ? `${user.firstName} ${user.lastName}` : "Unknown User"
  }
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
      newMessage.text = ""
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
      const res = await fetch(`${API_URL}/${id}`, { method: "DELETE" })
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

<main class="container">
  <div class="sidebar">
    <select bind:value={newMessage.userId}>
      <option value="" disabled selected>Select a User</option>
      {#each usersList as user}
        <option value={user.id}
          >{user.firstName} {user.lastName} ({user.email})</option
        >
      {/each}
    </select>
    <input
      type="number"
      bind:value={newMessage.channelId}
      placeholder="Channel ID"
    />
  </div>
  <div class="content">
    <div class="messages-area">
      {#each sortedMessages as message (message.id)}
        <div class="message">
          {#if editingMessage && editingMessage.id === message.id}
            <select bind:value={editingData.userId}>
              <option value="" disabled>Select a User</option>
              {#each usersList as user}
                <option value={user.id}
                  >{user.firstName} {user.lastName} ({user.email})</option
                >
              {/each}
            </select>
            <input
              type="text"
              bind:value={editingData.text}
              placeholder="Text"
            />
            <input
              type="number"
              bind:value={editingData.channelId}
              placeholder="Channel ID"
            />
            <button on:click={updateMessage}>Update</button>
            <button
              on:click={() => {
                editingMessage = null
              }}>Cancel</button
            >
          {:else}
            <div class="message-header">
              <div class="dropdown">
                <button
                  class="dropdown-toggle"
                  on:click={() =>
                    (openDropdownId =
                      openDropdownId === message.id ? null : message.id)}
                  >▼</button
                >
                {#if openDropdownId === message.id}
                  <div class="dropdown-menu">
                    <button
                      on:click={() => {
                        startEditing(message)
                        openDropdownId = null
                      }}>Edit</button
                    >
                    <button
                      on:click={() => {
                        deleteMessage(message.id)
                        openDropdownId = null
                      }}>Delete</button
                    >
                  </div>
                {/if}
              </div>
              <strong
                >{getUserName(message.userId)}
                {new Date(message.createdOn).toLocaleTimeString("en-US", {
                  hour: "numeric",
                  minute: "2-digit",
                })}</strong
              >
            </div>
            <div class="message-body">
              {message.text}
            </div>
          {/if}
        </div>
      {/each}
    </div>
    <div class="message-input">
      <input
        type="text"
        bind:value={newMessage.text}
        placeholder="Type your message here"
      />
      <button on:click={sendMessage}>Send</button>
    </div>
  </div>
</main>

<style>
  .container {
    display: flex;
    height: 100vh;
    padding: 1rem;
    box-sizing: border-box;
  }
  .sidebar {
    width: 20%;
    padding-right: 1rem;
    display: flex;
    flex-direction: column;
    justify-content: flex-start;
  }
  .content {
    width: 80%;
    display: flex;
    flex-direction: column;
  }
  .messages-area {
    flex: 1;
    overflow-y: auto;
    border: 1px solid #ddd;
    padding: 1rem;
    box-sizing: border-box;
  }
  .message-input {
    display: flex;
    padding: 1rem;
    border-top: 1px solid #ddd;
    box-sizing: border-box;
    position: sticky;
    bottom: 0;
    background: white;
  }
  .message-input input {
    flex: 1;
    padding: 0.5rem;
  }
  .message-input button {
    margin-left: 0.5rem;
  }
  .message {
    margin-bottom: 1rem;
  }
  .sidebar select,
  .sidebar input {
    margin-bottom: 1rem;
    padding: 0.5rem;
    width: 100%;
    box-sizing: border-box;
  }
  .message-header {
    display: flex;
    align-items: center;
    margin-bottom: 0.5rem;
  }
  .dropdown {
    position: relative;
    margin-right: 0.5rem;
  }
  .dropdown-toggle {
    background: none;
    border: none;
    cursor: pointer;
    font-size: 1rem;
  }
  .dropdown-menu {
    position: absolute;
    left: 0;
    top: 100%;
    background: white;
    border: 1px solid #ddd;
    z-index: 100;
    display: flex;
    flex-direction: column;
  }
  .dropdown-menu button {
    padding: 0.5rem 1rem;
    text-align: left;
    background: none;
    border: none;
    cursor: pointer;
  }
  .dropdown-menu button:hover {
    background: #f0f0f0;
  }
  .message-body {
    margin-bottom: 0.5rem;
  }
</style>
