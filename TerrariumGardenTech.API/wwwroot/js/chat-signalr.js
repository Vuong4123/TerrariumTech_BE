/**
 * SignalR Chat Client - Kết nối real-time, gửi/nhận tin nhắn, typing indicators, auto-reconnect
 */

class ChatSignalRClient {
    constructor(jwtToken) {
        this.connection = null;
        this.jwtToken = jwtToken;
        this.currentChatId = null;
        this.currentUserId = null;
        this.isConnected = false;
        
        // Callbacks cho UI
        this.onNewMessage = null;
        this.onUserOnline = null;
        this.onUserOffline = null;
        this.onTypingIndicator = null;
        this.onMessagesMarkedAsRead = null;
        this.onConnectionStateChanged = null;
        this.onError = null;
    }

    /**
     * Khởi tạo kết nối SignalR
     * 
     * Cấu hình connection với JWT authentication và auto-reconnect
     */
    async initialize() {
        try {
            // Tạo connection với JWT token
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/chatHub", {
                    accessTokenFactory: () => this.jwtToken
                })
                .withAutomaticReconnect([0, 2000, 10000, 30000]) // Auto-reconnect intervals
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // Đăng ký event handlers
            this.setupEventHandlers();

            await this.connection.start();
            this.isConnected = true;
            this.onConnectionStateChanged?.(true);

            return true;
        } catch (error) {
            console.error("SignalR connection failed:", error);
            this.onError?.("Failed to connect to chat server");
            return false;
        }
    }

    /**
     * Thiết lập các event handlers cho SignalR
     */
    setupEventHandlers() {
        this.connection.on("NewMessage", (message) => {
            this.onNewMessage?.(message);
        });

        this.connection.on("UserOnline", (userId) => {
            this.onUserOnline?.(userId);
        });

        this.connection.on("UserOffline", (userId) => {
            this.onUserOffline?.(userId);
        });

        this.connection.on("TypingIndicator", (data) => {
            this.onTypingIndicator?.(data);
        });

        this.connection.on("MessagesMarkedAsRead", (data) => {
            this.onMessagesMarkedAsRead?.(data);
        });

        this.connection.on("UserJoinedChat", (data) => {
            // Optional: handle user joined
        });

        this.connection.on("UserLeftChat", (data) => {
            // Optional: handle user left
        });

        this.connection.on("Error", (message) => {
            this.onError?.(message);
        });

        this.connection.onreconnecting(() => {
            this.isConnected = false;
            this.onConnectionStateChanged?.(false);
        });

        this.connection.onreconnected(() => {
            this.isConnected = true;
            this.onConnectionStateChanged?.(true);

            if (this.currentChatId) {
                this.joinChatRoom(this.currentChatId);
            }
        });

        this.connection.onclose(() => {
            this.isConnected = false;
            this.onConnectionStateChanged?.(false);
        });
    }

    /**
     * Join vào chat room để nhận tin nhắn real-time
     * 
     * @param {number} chatId - ID của chat cần join
     */
    async joinChatRoom(chatId) {
        if (!this.isConnected) {
            console.warn("⚠️ Not connected to SignalR");
            return false;
        }

        try {
            await this.connection.invoke("JoinChatRoom", chatId);
            this.currentChatId = chatId;
            return true;
        } catch (error) {
            this.onError?.("Failed to join chat room");
            return false;
        }
    }

    /**
     * Leave khỏi chat room hiện tại
     */
    async leaveChatRoom() {
        if (!this.isConnected || !this.currentChatId) {
            return;
        }

        try {
            await this.connection.invoke("LeaveChatRoom", this.currentChatId);
            this.currentChatId = null;
        } catch (error) {
            // Silent fail for leave room
        }
    }

    /**
     * Gửi tin nhắn real-time
     * 
     * @param {number} chatId - ID của chat
     * @param {string} content - Nội dung tin nhắn
     */
    async sendMessage(chatId, content) {
        if (!this.isConnected) {
            this.onError?.("Not connected to chat server");
            return false;
        }

        if (!content || content.trim().length === 0) {
            this.onError?.("Message content cannot be empty");
            return false;
        }

        if (content.length > 1000) {
            this.onError?.("Message too long (max 1000 characters)");
            return false;
        }

        try {
            await this.connection.invoke("SendMessage", chatId, content.trim());
            return true;
        } catch (error) {
            this.onError?.("Failed to send message");
            return false;
        }
    }

    /**
     * Đánh dấu tin nhắn đã đọc
     * 
     * @param {number} chatId - ID của chat
     */
    async markMessagesAsRead(chatId) {
        if (!this.isConnected) return;

        try {
            await this.connection.invoke("MarkMessagesAsRead", chatId);
        } catch (error) {
            // Silent fail for read receipts
        }
    }

    /**
     * Gửi typing indicator
     * 
     * @param {number} chatId - ID của chat
     * @param {boolean} isTyping - True nếu đang gõ, false nếu dừng
     */
    async sendTypingIndicator(chatId, isTyping) {
        if (!this.isConnected) return;

        try {
            await this.connection.invoke("SendTypingIndicator", chatId, isTyping);
        } catch (error) {
            // Silent fail for typing indicators
        }
    }

    /**
     * Ngắt kết nối SignalR
     */
    async disconnect() {
        if (this.connection) {
            await this.leaveChatRoom();
            await this.connection.stop();
            this.isConnected = false;
        }
    }

    /**
     * Kiểm tra trạng thái kết nối
     */
    getConnectionState() {
        return {
            isConnected: this.isConnected,
            state: this.connection?.state || "Disconnected",
            currentChatId: this.currentChatId
        };
    }
}

// Export cho sử dụng
window.ChatSignalRClient = ChatSignalRClient;

/**
 * USAGE EXAMPLE:
 * 
 * // 1. Khởi tạo client
 * const chatClient = new ChatSignalRClient(jwtToken);
 * 
 * // 2. Thiết lập callbacks
 * chatClient.onNewMessage = (message) => {
 *     displayMessage(message);
 * };
 * 
 * chatClient.onTypingIndicator = (data) => {
 *     showTypingIndicator(data.userId, data.isTyping);
 * };
 * 
 * chatClient.onConnectionStateChanged = (isConnected) => {
 *     updateConnectionStatus(isConnected);
 * };
 * 
 * // 3. Kết nối
 * await chatClient.initialize();
 * 
 * // 4. Join chat room
 * await chatClient.joinChatRoom(chatId);
 * 
 * // 5. Gửi tin nhắn
 * await chatClient.sendMessage(chatId, "Hello!");
 * 
 * // 6. Cleanup khi không dùng
 * await chatClient.disconnect();
 */
