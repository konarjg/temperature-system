# (Your Dockerfile from the 'build' stage is above this)

# Stage 3: The Final Image - Copies the published output to the lean base image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# --- THIS IS THE NEW, CRUCIAL SECTION FOR SECRETS ---
ARG JWT_SECRET_KEY
ARG EMAIL_SENDER_PASSWORD
ENV Jwt__Key=$JWT_SECRET_KEY
ENV Email__SenderPassword=$EMAIL_SENDER_PASSWORD
# ---------------------------------------------------

# --- PERMISSIONS FIX (QUICK METHOD) ---
# Give the 'app' user ownership of the entire application directory
RUN chown -R app:app /app
# --------------------------------------

# --- SWITCH TO NON-ROOT USER ---
USER app
# -------------------------------

ENTRYPOINT ["dotnet", "TemperatureSystem.dll"]