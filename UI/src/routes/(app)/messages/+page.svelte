<script>
  import { onMount, tick } from "svelte"
  const API_URL = "https://localhost:44333/api/messages"
  let messages = []
  let error = ""
  let newMessage = { userId: "", text: "", channelId: 1 }
  let editingMessage = null
  let editingData = { text: "" }
  let usersList = []
  let openDropdownId = null
  let messagesArea
  $: sortedMessages = messages
    .slice()
    .sort((a, b) => new Date(a.createdOn) - new Date(b.createdOn))
  $: displayedMessages = newMessage.channelId
    ? sortedMessages.filter(
        (m) => Number(m.channelId) === Number(newMessage.channelId),
      )
    : []
  function groupMessagesByDate(messages) {
    const groups = []
    let currentGroup = null
    messages.forEach((message) => {
      const date = new Date(message.createdOn).toLocaleDateString()
      if (!currentGroup || currentGroup.date !== date) {
        currentGroup = { date, messages: [] }
        groups.push(currentGroup)
      }
      currentGroup.messages.push(message)
    })
    return groups
  }
  $: groupedMessages = groupMessagesByDate(displayedMessages)
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
      if (usersList.length > 0 && !newMessage.userId) {
        newMessage.userId = usersList[0].id
      }
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
      await tick()
      if (messagesArea) {
        messagesArea.scrollTop = messagesArea.scrollHeight
      }
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
    editingData.text = message.text
  }
  function handleEditKeydown(event) {
    if (event.key === "Enter") updateMessage()
  }
  async function updateMessage() {
    if (!editingData.text.trim()) return
    try {
      const res = await fetch(`${API_URL}/${editingMessage.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          userId: editingMessage.userId,
          text: editingData.text,
          channelId: editingMessage.channelId,
        }),
      })
      if (!res.ok) {
        error = `Failed to update message: ${res.status}`
        return
      }
      editingMessage = null
      editingData.text = ""
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
  function handleSendKeydown(event) {
    if (event.key === "Enter") {
      event.preventDefault()
      sendMessage()
    }
  }
</script>

<main class="container">
  <div class="sidebar">
    <label for="user-select">User</label>
    <select id="user-select" bind:value={newMessage.userId}>
      <option value="" disabled>Select a User</option>
      {#each usersList as user}
        <option value={user.id}>
          {user.firstName}
          {user.lastName} ({user.email})
        </option>
      {/each}
    </select>
    <label for="channel-input">Channel</label>
    <input
      id="channel-input"
      type="number"
      bind:value={newMessage.channelId}
      placeholder="Channel ID"
    />
  </div>
  <div class="content">
    <div class="messages-area" bind:this={messagesArea}>
      {#each groupedMessages as group}
        <div class="group-header">{group.date}</div>
        {#each group.messages as message (message.id)}
          <div class="message">
            {#if editingMessage && editingMessage.id === message.id}
              <div class="message-header">
                <strong>
                  {getUserName(message.userId)}{" "}
                  {new Date(message.createdOn).toLocaleTimeString("en-US", {
                    hour: "numeric",
                    minute: "2-digit",
                  })}
                </strong>
              </div>
              <div class="message-body">
                <input
                  type="text"
                  bind:value={editingData.text}
                  placeholder="Text"
                  on:keydown={handleEditKeydown}
                />
                <button on:click={updateMessage}>Save</button>
                <button
                  on:click={() => {
                    editingMessage = null
                  }}
                >
                  Cancel
                </button>
              </div>
            {:else}
              <div class="message-header">
                <div class="dropdown">
                  <button
                    class="dropdown-toggle"
                    on:click={() =>
                      (openDropdownId =
                        openDropdownId === message.id ? null : message.id)}
                  >
                    ▼
                  </button>
                  {#if openDropdownId === message.id}
                    <div class="dropdown-menu">
                      <button
                        on:click={() => {
                          startEditing(message)
                          openDropdownId = null
                        }}
                      >
                        Edit
                      </button>
                      <button
                        on:click={() => {
                          deleteMessage(message.id)
                          openDropdownId = null
                        }}
                      >
                        Delete
                      </button>
                    </div>
                  {/if}
                </div>
                <strong>
                  {getUserName(message.userId)}{" "}
                  {new Date(message.createdOn).toLocaleTimeString("en-US", {
                    hour: "numeric",
                    minute: "2-digit",
                  })}
                </strong>
              </div>
              <div class="message-body">{message.text}</div>
            {/if}
          </div>
        {/each}
      {/each}
    </div>
    <div class="message-input">
      <input
        type="text"
        bind:value={newMessage.text}
        placeholder="Type your message here"
        on:keyup={handleSendKeydown}
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
  .sidebar label {
    margin-bottom: 0.25rem;
    font-weight: bold;
  }
  .sidebar select,
  .sidebar input {
    margin-bottom: 1rem;
    padding: 0.5rem;
    width: 100%;
    box-sizing: border-box;
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
    scrollbar-width: none;
    -ms-overflow-style: none;
    background: #f0f0f0;
  }
  .messages-area::-webkit-scrollbar {
    display: none;
  }
  .group-header {
    text-align: center;
    border-bottom: 1px solid #ccc;
    margin: 1rem 0;
    padding-bottom: 0.5rem;
    font-weight: bold;
  }
  .message {
    margin-bottom: 1rem;
    padding: 0.5rem;
    border-radius: 4px;
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
</style>
