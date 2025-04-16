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

<main class="flex h-screen p-4">
  <!-- Sidebar -->
  <aside class="w-1/5 pr-4 flex flex-col space-y-4">
    <label class="label">
      <span class="label-text font-bold">User</span>
    </label>
    <select
      id="user-select"
      bind:value={newMessage.userId}
      class="select select-bordered w-full"
    >
      <option value="" disabled>Select a User</option>
      {#each usersList as user}
        <option value={user.id}>
          {user.firstName}
          {user.lastName} ({user.email})
        </option>
      {/each}
    </select>

    <label class="label">
      <span class="label-text font-bold">Channel</span>
    </label>
    <input
      id="channel-input"
      type="number"
      bind:value={newMessage.channelId}
      placeholder="Channel ID"
      class="input input-bordered w-full"
    />
  </aside>

  <!-- Content -->
  <section class="flex-1 flex flex-col">
    <!-- Messages Area -->
    <div
      class="flex-1 overflow-y-auto bg-secondary p-4 space-y-4"
      bind:this={messagesArea}
    >
      {#each groupedMessages as group}
        <div class="text-center font-bold border-b border-gray-300 pb-2 my-4">
          {group.date}
        </div>

        {#each group.messages as message (message.id)}
          {#if editingMessage && editingMessage.id === message.id}
            <!-- Edit Mode -->
            <div class="p-4 bg-white rounded-lg shadow">
              <div class="flex items-center mb-2">
                <strong class="font-semibold">
                  {getUserName(message.userId)}{" "}
                  <span class="text-sm text-gray-500 ml-1">
                    {new Date(message.createdOn).toLocaleTimeString("en-US", {
                      hour: "numeric",
                      minute: "2-digit",
                    })}
                  </span>
                </strong>
              </div>
              <div class="flex space-x-2">
                <input
                  type="text"
                  bind:value={editingData.text}
                  placeholder="Text"
                  on:keydown={handleEditKeydown}
                  class="input input-bordered flex-1"
                />
                <button on:click={updateMessage} class="btn btn-sm btn-primary">
                  Save
                </button>
                <button
                  on:click={() => (editingMessage = null)}
                  class="btn btn-sm"
                >
                  Cancel
                </button>
              </div>
            </div>
          {:else}
            <!-- Display Mode with down‑arrow trigger -->
            <div class="p-4 bg-white rounded-lg shadow">
              <div class="flex items-center mb-2">
                <strong class="font-semibold">
                  {getUserName(message.userId)}
                  <span class="text-sm text-gray-500 ml-1">
                    {new Date(message.createdOn).toLocaleTimeString("en-US", {
                      hour: "numeric",
                      minute: "2-digit",
                    })}
                  </span>
                </strong>

                <div class="dropdown inline-block relative ml-1">
                  <button
                    class="btn btn-ghost btn-xs p-0"
                    aria-label="Actions"
                    on:click={() =>
                      (openDropdownId =
                        openDropdownId === message.id ? null : message.id)}
                  >
                    ▾
                  </button>

                  {#if openDropdownId === message.id}
                    <ul
                      class="absolute left-0 top-full mt-1 menu p-2 shadow bg-base-100 rounded-box w-32 z-10"
                    >
                      <li>
                        <a
                          on:click={() => {
                            startEditing(message)
                            openDropdownId = null
                          }}>Edit</a
                        >
                      </li>
                      <li>
                        <a
                          on:click={() => {
                            deleteMessage(message.id)
                            openDropdownId = null
                          }}>Delete</a
                        >
                      </li>
                    </ul>
                  {/if}
                </div>
              </div>
              <div class="mb-2">{message.text}</div>
            </div>
          {/if}
        {/each}
      {/each}
    </div>

    <!-- Message Input -->
    <div class="mt-auto flex border-t border-gray-200 py-4 bg-base-100">
      <input
        type="text"
        bind:value={newMessage.text}
        placeholder="Type your message here"
        on:keyup={handleSendKeydown}
        class="input input-bordered flex-1 mr-2"
      />
      <button on:click={sendMessage} class="btn btn-primary">Send</button>
    </div>
  </section>
</main>
