# User stories

## 1. Data fetching

As a user, I want the system to automatically fetch and store ARKK data so that I always have access to up-to-date portfolio information.

### 1.1. Acceptance criteria

- Runs every day at 11:32:28.
- Data is successfully fetched and parsed from the csv available on https://www.ark-funds.com/funds/arkk.
- Data is stored in a database.
- Data is exposed to clients via a REST endpoint.

### 1.2. Out of scope

- Calculating diffs between dates.

### 1.3. Estimate

2 MD

## 2. Comparing holdings in time

As a user, I want to retrieve ARKK holdings for two selected dates and compare them so that I can see changes in the portfolio over time.

### 2.1. Acceptance criteria

- Client can request comparison for two dates.
- System fetches holdings according to selected dates.
- System returns desired holdings.
- User can see diff between those two reports.

### 2.2. Out of scope

- Calculating percentage changes.
- Change visualisation.

### 2.3. Estimate

3 MD

## 3. User authentication (Sign in)

As a user, I want to sign in using a username and password in the CLI application so that I can access the system.

### 3.1. Acceptance criteria

- When the application starts, the user is prompted to enter username and password.
- Password input is hidden while typing.
- System validates the credentials against the predefined users stored in the system.
- If credentials are valid, the user is successfully signed in and can access the application features.
- If credentials are invalid, the user is shown an error message and can try again.
- After successful authentication, the user session remains active until the application is closed or the user logs out.

### 3.2. Out of scope

- User registration.
- User management interface (users are created manually by the developers).
- Password reset functionality.
- Role/permission management.

### 3.3. Estimate

2 MD

## 4. See recent data

As a signed-in user, I want to view the most recent available data in the CLI application so that I can quickly see the latest information retrieved by the system.

### 4.1. Acceptance criteria

- After successful sign-in, the user can choose an option to view the most recent data.
- The system loads the most recently downloaded dataset stored locally.
- The data is displayed in a clear and readable table format in the terminal UI.
- The terminal output uses a clean, visually structured layout suitable for a console GUI.
- The displayed data represents the latest available dataset downloaded automatically by the system.
- If no data is available yet, the user is informed that no dataset has been downloaded.

### 4.2. Out of scope

- Graphical charts or plots.
- Editing or modifying the dataset.
- Data comparison between different dates (handled in another feature).
- Manual data download by the user.

### 4.3. Estimate

1.5 MD

## 5. User registration

As a new user, I want to create an account by providing a username and password in the CLI application so that I can securely access the system's features.

### 5.1. Acceptance criteria

- When the application starts, the user has an option to choose "Register" instead of "Sign In".
- The system prompts the user to enter a desired username and a password.
- Password input is hidden while typing.
- The system checks if the username is already taken; if it is, an error message is displayed, and the user can try again.
- The system validates that the password meets minimum security requirements (e.g., length).
- Upon successful registration, the new user credentials are saved in the system, and the user is redirected to the sign-in screen or automatically logged in.

### 5.2. Out of scope

- Email verification or multi-factor authentication (MFA).
- Social media login (OAuth).
- Advanced password strength meters.

### 5.3. Estimate

1.5 MD

## 6. System Deployment and Connectivity

As a developer, I want to set up a distributed architecture and a public distribution point so that users can download the application and access synchronized data from any location.

### 6.1. Acceptance criteria

- The system provides a public web page where the client can download the latest version of the CLI application as an executable (.exe) file.
- The CLI application communicates with a remote server via an API.
- The server communicates with a database to store and retrieve historical ARK holdings and user credentials.
- An independent background service runs on the server to handle the automated daily data download at 11:32:28 AM.
- The deployment process includes the execution of unit and integration tests to ensure system stability.
- All communication between the CLI client and the server requires authentication based on the user's credentials.

### 6.2. Out of scope

- Automatic updates of the .exe file (auto-updater).
- High availability (HA) cluster setup or load balancing.
- Web-based dashboard for data visualization.

### 6.3. Estimate

4 MD

## 7. Automated testing

As a developer, I want to automatically verify that my changes do not break existing functionality so that I can confidently deliver new features and maintain system stability.

### 7.1. Acceptance criteria

The system includes automated unit tests for core business logic (e.g., data parsing, comparison logic, authentication).

The system includes integration tests for key workflows (e.g., fetching and storing ARKK data, API communication, authentication flow).

Tests can be executed locally using a single command.

Tests are automatically executed as part of the build/deployment process.

If any test fails, the build/deployment process is marked as failed.

Test results are clearly reported (success/failure, logs).

Test coverage is measurable (e.g., via coverage report).

### 7.2. Out of scope

End-to-end UI testing (CLI interaction simulation).

Performance/load testing.

Security/penetration testing.

### 7.3. Estimate

2–3 MD

## 8. Project setup and development environment

As a developer, I want to set up the project structure and development environment so that I can efficiently develop, run, and maintain the application.

### 8.1. Acceptance criteria

The project repository is initialized (e.g., Git) with a clear folder structure.

The project includes basic documentation (README) with setup and run instructions.

The application can be built and run locally.

Environment variables are configurable (e.g., via .env file).

Database is configurable and can be started locally (e.g., via Docker).

Basic project dependencies are installed and managed (e.g., package manager).

Linting and formatting tools are configured.

The project includes scripts for common tasks (e.g., start, build, test).

### 8.2. Out of scope

CI/CD pipeline setup (covered in testing/deployment stories).

Advanced infrastructure configuration (cloud, scaling).

Production-ready security hardening.

### 8.3. Estimate

1.5–2 MD
